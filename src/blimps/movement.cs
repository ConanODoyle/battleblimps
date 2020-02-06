function GameConnection::controlBlimp(%cl, %blimp)
{
	if (!%cl.controllingBlimp || %cl.camera.isSpying != %blimp)
	{
		%cl.camera.setControlObject(%cl.camera); //must be first - pkg resetCamera resets subsequent lines

		%cl.camera.schedule(1, setOrbitMode, %blimp, %blimp.getTransform(), 0, 10, 10, 1);
		%cl.camera.isSpying = %blimp;
		%cl.setControlObject(%cl.camera);
		%cl.camera.mode = "Orbit";
	}

	%cl.blimpFreelook = 0;
	%cl.controllingBlimp = %blimp;
	%cl.upMovement = %cl.downMovement = %cl.forwardMovement = %cl.backMovement = 0;
	%blimp.lastMoved = getSimTime();

	blimpControlTick(%blimp, %cl);
}

function blimpControlTick(%blimp, %cl)
{
	cancel(%cl.blimpControlSched);

	//end control if camera is not in control mode
	if (%cl.controllingBlimp != %blimp || %cl.camera.mode !$= "Orbit" || !isObject(%cl.camera) || !isObject(%blimp))
	{
		%cl.camera.isSpying = 0;
		%cl.controllingBlimp = 0;
		%cl.camera.setMode("Observer");
		return;
	}

	%timeSinceLastMove = getSimTime() - %blimp.lastMoved;
	%timeSinceLastPrint = getSimTime() - %cl.lastPrintControls;
	%blimp.lastMoved = getSimTime();
	%originalVelocity = %blimp.getVelocity();
	%forwardDir = %blimp.getForwardVector();
	%factor = %timeSinceLastMove / 1000;
	%addedVelocity = "0 0 0";

	%upAcceleration 		= %blimp.upAcceleration * %factor;
	%downAcceleration 		= %blimp.downAcceleration * %factor;
	%forwardAcceleration 	= %blimp.forwardAcceleration * %factor;
	%backwardAcceleration 	= %blimp.backwardAcceleration * %factor;
	%maxHorizontalSpeed 	= %blimp.maxHorizontalSpeed;
	%maxVerticalSpeed 		= %blimp.maxVerticalSpeed;
	%driftFactor 			= %blimp.driftFactor;

	//update blimp rotation
	if (!%cl.blimpFreelook)
	{
		%flatvec = vectorNormalize(getWords(%cl.camera.getEyeVector(), 0, 1));
		%blimp.setAimVector(%flatvec);
		//prevents rotation updates not being sent to client due to slow turns
		%blimp.addVelocity("0 0 0.01");
		%blimp.addVelocity("0 0 -0.01");
	}
	else
	{
		%blimp.clearAim();
	}

	//move blimp
	if (%cl.upMovement && !%cl.downMovement)
	{
		%addedVelocity = vectorAdd(%addedVelocity, "0 0 " @ %upAcceleration);
	}
	else if (%cl.downMovement && !%cl.upMovement)
	{
		%addedVelocity = vectorAdd(%addedVelocity, "0 0 " @ %downAcceleration);
	}

	if (%cl.forwardMovement && !%cl.backMovement)
	{
		%addedVelocity = vectorAdd(%addedVelocity, vectorScale(%forwardDir, %forwardAcceleration));
		%playThread = "run";
	}
	else if (%cl.backMovement && !%cl.forwardMovement)
	{
		%addedVelocity = vectorAdd(%addedVelocity, vectorScale(%forwardDir, %backwardAcceleration));
		%playThread = "back";
	}
	else if (%blimp.lastThread !$= "root")
	{
		%playThread = "root";
	}

	//play the correct thread
	if (%blimp.lastThread !$= %playThread && %playThread !$= "")
	{
		%blimp.lastThread = %playThread;
		%blimp.playThread(0, %playThread);
	}

	%prefinalVelocity = vectorAdd(%originalVelocity, %addedVelocity);

	//clamp and check z velocity
	//do not accelerate if higher than max (with added velocity)
	%z = getWord(%prefinalVelocity, 2);
	%addedVelocity = mAbs(%z) > %maxVerticalSpeed ? getWords(%addedVelocity, 0, 1) : %addedVelocity;
	
	//clamp and fix horiz velocity
	//do not accelerate if higher than max (with added velocity)
	%horizVel = getWords(%originalVelocity, 0, 1);
	%horizVelProj = vectorScale(%forwardDir, vectorDot(%horizVel, %forwardDir));
	%adjustedHVector = vectorAdd(vectorScale(%horizVel, %driftFactor), vectorScale(%horizVelProj, 1 - %driftFactor));
	%finalHVector = vectorAdd(getWords(%addedVelocity, 0, 1), %adjustedHVector);
	if (vectorLen(%finalHVector) > %maxHorizontalSpeed)
	{
		%addedVelocity = "0 0 " @ getWord(%addedVelocity, 2);
	}

	%finalVelocity = vectorAdd(getWords(%adjustedHVector, 0, 1) SPC getWord(%originalVelocity, 2), %addedVelocity);
	%blimp.setVelocity(%finalVelocity);

	if (%blimp.debugVelocity)
	{
		%name = "H: " @ vectorLen(getWords(%finalVelocity, 0, 1)) @ " V:" @ getWord(%finalVelocity, 2);
		%name = %name @ " | Max H/V: " @ %maxHorizontalSpeed SPC %maxVerticalSpeed;
		%blimp.setShapeName(%name, 8564862);
		if (%blimp.echoDebug)
		{
			echo(%name);
		}
	}

	if (%timeSinceLastPrint > 50)
	{
		%cl.lastPrintControls = getSimTime();
		%cl.centerprintBlimpControl();
	}

	%cl.blimpControlSched = schedule(1, %blimp, blimpControlTick, %blimp, %cl);
}

function toggleFreeLook(%cl)
{
	if (getSimTime() - %cl.lastToggleFreelook < 50)
	{
		return;
	}
	%cl.lastToggleFreelook = getSimTime();
	%cl.blimpFreelook = !%cl.blimpFreelook;
	%cl.centerprintBlimpControl();
}

function GameConnection::centerprintBlimpControl(%cl)
{
	%blimp = %cl.controllingBlimp;
	if (!isObject(%blimp))
	{
		return;
	}

	%velocity = %blimp.getVelocity();
	%horizontalSpeed = vectorLen(getWords(%velocity, 0, 1));
	%verticalSpeed = getWord(%velocity, 2);
	%freelook = %cl.blimpFreelook;
	%maxHorizontalSpeed 	= %blimp.maxHorizontalSpeed * 10;
	%maxVerticalSpeed 		= %blimp.maxVerticalSpeed * 10;
	%driftFactor 			= %blimp.driftFactor;
	%forward = %cl.forwardMovement ? "FWD " : "";
	%backward = %cl.backMovement ? "BCK " : "";
	%up = %cl.upMovement ? "UP " : "";
	%down = %cl.downMovement ? "DWN " : "";
	%none = %forward @ %backward @ %up @ %down !$= "" ? "" : "\c3OFF";

	%format = "<just:right><font:Consolas:18>";
	%throttle = "\c5Throttle: \c6[\c2" @ trim(%forward @ %backward @ %up @ %down @ %none) @ "\c6]";
	%hspeedometer = "\c5Horiz. Speed: \c6[\c2" @ mFloor(%horizontalSpeed * 10 + 0.5) @ " / " @ %maxHorizontalSpeed @ "\c6]";
	%vspeedometer = "\c5Vert. Speed: \c6[\c2" @ mFloor(%verticalSpeed * 10 + 0.5) @ " / " @ %maxVerticalSpeed @ "\c6]";
	%freelook = %freelook ? "\c0-FREELOOK ON-" : "";
	%rf = " <br>";
	%cl.centerprint(%format @ %throttle @ %rf @ %hspeedometer @ %rf @ %vspeedometer @ %rf @ %freelook, 1);
}

package resetCamera
{
	function GameConnection::setControlObject(%cl, %obj)
	{
		if (%cl.controllingBlimp)
		{
			%cl.camera.isSpying = 0;
			%cl.controllingBlimp = 0;
			%cl.camera.setMode("Observer");
		}
		return parent::setControlObject(%cl, %obj);
	}

	function Observer::onTrigger(%this, %obj, %trigger, %state)
	{
		%cl = %obj.getControllingClient();
		if (%cl.controllingBlimp)
		{
			//trig 2 = spacebar
			//trig 3 = shift
			//trig 4 = rmb
			switch (%trigger)
			{
				case 0: %cl.forwardMovement = %state;
				case 2: %cl.upMovement = %state;
				case 3: %cl.downMovement = %state;
				case 4: %cl.backMovement = %state;
			}
		}
		parent::onTrigger(%this, %obj, %trigger, %state);
	}

	function serverCmdShiftBrick(%cl, %x, %y, %z)
	{
		if (%cl.controllingBlimp)
		{
			toggleFreeLook(%cl);
			return;
		}
		parent::serverCmdShiftBrick(%cl, %x, %y, %z);
	}

	function serverCmdRotateBrick(%cl, %rot)
	{
		if (%cl.controllingBlimp)
		{
			toggleFreeLook(%cl);
			return;
		}
		parent::serverCmdRotateBrick(%cl, %rot);
	}

	function serverCmdPlantBrick(%cl)
	{
		if (%cl.controllingBlimp)
		{
			toggleFreeLook(%cl);
			return;
		}
		parent::serverCmdPlantBrick(%cl);
	}

	function serverCmdCancelBrick(%cl)
	{
		if (%cl.controllingBlimp)
		{
			toggleFreeLook(%cl);
			return;
		}
		parent::serverCmdCancelBrick(%cl);
	}
};
activatePackage(resetCamera);

function initZeroGravZone()
{
	if (!isObject($ZeroGravZone))
	{
		$ZeroGravZone = new PhysicalZone(ZeroGravZone)
		{
			position = "-10000 10000 0";
			velocityMod = "1";
			gravityMod = "0";
			extraDrag = "0";
			isWater = "1";
			waterViscosity = "2";
			waterColor = "0.000000 0.000000 0.000000 0.000000";
			appliedForce = "0 0 0";
			polyhedron = "0.0 0.0 0.0 1.0 0.0 0.0 0.0 -1.0 0.0 0.0 0.0 1.0";
			scale = "20000 20000 20000";
		};
	}
}

function serverCmdToggleZeroGrav(%cl)
{
	if (%cl.isAdmin)
	{
		if (isObject($ZeroGravZone))
		{
			$ZeroGravZone.delete();
			announce("Zero gravity zone disabled by " @ %cl.name);
		}
		else
		{
			initZeroGravZone();
			announce("Zero gravity zone enabled by " @ %cl.name);
		}
	}
}
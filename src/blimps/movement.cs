function GameConnection::controlAircraft(%cl, %aircraft)
{
	if (!%cl.controllingAircraft || %cl.camera.isSpying != %aircraft)
	{
		%cl.camera.setControlObject(%cl.camera); //must be first - pkg resetCamera resets subsequent lines

		%dist = %aircraft.cameraDistance > 0 ? %aircraft.cameraDistance : 10;
		%cl.camera.schedule(1, setOrbitMode, %aircraft, %aircraft.getTransform(), 0, %dist, %dist, 1);
		%cl.camera.isSpying = %aircraft;
		%cl.setControlObject(%cl.camera);
		%cl.camera.mode = "Orbit";
	}

	%cl.aircraftFreeLook = 0;
	%cl.controllingAircraft = %aircraft;
	%cl.upMovement = %cl.downMovement = %cl.forwardMovement = %cl.backMovement = 0;
	%aircraft.lastMoved = getSimTime();

	if (%aircraft.isBlimp)
	{
		blimpControlTick(%aircraft, %cl);
	}
	else if (%aircraft.isPlane)
	{
		planeControlTick(%aircraft, %cl);
	}
}

function blimpControlTick(%blimp, %cl)
{
	cancel(%cl.aircraftControlSched);

	//end control if camera is not in control mode
	if (%cl.controllingAircraft != %blimp || %cl.camera.mode !$= "Orbit" || !isObject(%cl.camera) || !isObject(%blimp))
	{
		%cl.camera.isSpying = 0;
		%cl.controllingAircraft = 0;
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
	if (!%cl.aircraftFreeLook)
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

	%cl.aircraftControlSched = schedule(1, %blimp, blimpControlTick, %blimp, %cl);
}

function planeControlTick(%plane, %cl)
{
	cancel(%cl.aircraftControlSched);

	//end control if camera is not in control mode
	if (%cl.controllingAircraft != %plane || %cl.camera.mode !$= "Orbit" || !isObject(%cl.camera) || !isObject(%plane))
	{
		%cl.camera.isSpying = 0;
		%cl.controllingAircraft = 0;
		%cl.camera.setMode("Observer");
		return;
	}

	%timeSinceLastMove = getSimTime() - %plane.lastMoved;
	%timeSinceLastPrint = getSimTime() - %cl.lastPrintControls;
	%plane.lastMoved = getSimTime();
	%originalVelocity = %plane.getVelocity();
	%eyeDir = %plane.getEyeVector();
	%factor = %timeSinceLastMove / 1000;
	%addedVelocity = "0 0 0";

	%eyeAcceleration 		= %plane.eyeAcceleration * %factor;
	%passiveAcceleration 	= %plane.passiveAcceleration * %factor;
	%maxUpSpeed	 			= %plane.maxUpSpeed;
	%maxSpeed 				= %plane.maxSpeed;
	%minSpeed 				= %plane.minSpeed;
	%driftFactor 			= %plane.driftFactor;

	//update plane rotation
	if (!%cl.planeFreelook)
	{
		// %flatvec = vectorNormalize(getWords(%cl.camera.getEyeVector(), 0, 1));
		%eyeVec = %cl.camera.getEyeVector();
		%plane.setAimVector(%eyeVec);
		//prevents rotation updates not being sent to client due to slow turns
		%plane.addVelocity("0 0 0.01");
		%plane.addVelocity("0 0 -0.01");
	}
	else
	{
		%plane.clearAim();
	}

	//move plane
	if (%cl.forwardMovement && !%cl.backMovement)
	{
		%addedVelocity = vectorAdd(%addedVelocity, vectorScale(%eyeDir, %eyeAcceleration));
		%playThread = "runfast";
	}
	else if (%cl.backMovement && !%cl.forwardMovement)
	{
		%playThread = "runslow";
	}
	else
	{
		%addedVelocity = vectorAdd(%addedVelocity, vectorScale(%eyeDir, %passiveAcceleration));
		%playThread = "run";
	}

	//play the correct thread
	if (%plane.lastThread !$= %playThread && %playThread !$= "")
	{
		%plane.lastThread = %playThread;
		%plane.playThread(0, %playThread);
	}
	
	//clamp and fix velocity
	//do not accelerate if higher than max (with added velocity)
	if (vectorDot(%originalVelocity, %eyeDir) < 0)
	{
		%originalVelocity = vectorScale(%eyeDir, 0.01);
	}

	%origVelProj = vectorScale(%eyeDir, vectorDot(%originalVelocity, %eyeDir));
	%adjustedVector = vectorAdd(vectorScale(%originalVelocity, %driftFactor), vectorScale(%origVelProj, 1 - %driftFactor));
	%finalVector = vectorAdd(%addedVelocity, %adjustedVector);
	%record = %finalVector;

	if (vectorLen(%finalVector) > %maxSpeed)
	{
		%finalVector = vectorScale(vectorNormalize(%finalVector), %maxSpeed);
	}
	else if (vectorLen(%finalVector) < %minSpeed)
	{
		%finalVector = vectorScale(vectorNormalize(%finalVector), %minSpeed);
	}

	if (getWord(%finalVector, 2) > %maxUpSpeed) //only limit upward speed
	{
		%finalVector = getWords(%finalVector, 0, 1) SPC %maxUpSpeed; 
		%plane.reducingcount = %finalVector;
	}

	%finalVelocity = %finalVector;
	%plane.setVelocity(%finalVelocity);

	if (%plane.debugVelocity)
	{
		%name = "H: " @ vectorLen(getWords(%finalVelocity, 0, 1)) @ " V:" @ getWord(%finalVelocity, 2);
		%name = %name @ " | Previous: " @ %plane.reducingcount;
		%plane.setShapeName(%name, 8564862);
		if (%plane.echoDebug)
		{
			echo(%name);
		}
	}

	if (%timeSinceLastPrint > 50)
	{
		%cl.lastPrintControls = getSimTime();
		%cl.centerprintPlaneControl();
	}

	%cl.aircraftControlSched = schedule(1, %plane, planeControlTick, %plane, %cl);
}


function toggleFreeLook(%cl)
{
	if (getSimTime() - %cl.lastToggleFreelook < 50)
	{
		return;
	}
	%cl.lastToggleFreelook = getSimTime();
	%cl.aircraftFreeLook = !%cl.aircraftFreeLook;
	%cl.centerprintBlimpControl();
}

function GameConnection::centerprintBlimpControl(%cl)
{
	%blimp = %cl.controllingAircraft;
	if (!isObject(%blimp) || !%blimp.isBlimp)
	{
		return;
	}

	%velocity = %blimp.getVelocity();
	%horizontalSpeed = vectorLen(getWords(%velocity, 0, 1));
	%verticalSpeed = getWord(%velocity, 2);
	%freelook = %cl.aircraftFreeLook;
	%maxHorizontalSpeed 	= %blimp.maxHorizontalSpeed * 10;
	%maxVerticalSpeed 		= %blimp.maxVerticalSpeed * 10;
	%driftFactor 			= %blimp.driftFactor;
	%forward = %cl.forwardMovement && !%cl.backMovement ? "FWD " : "";
	%backward = %cl.backMovement && !%cl.forwardMovement ? "BCK " : "";
	%up = %cl.upMovement && !%cl.downMovement ? "UP " : "";
	%down = %cl.downMovement && !%cl.upMovement ? "DWN " : "";
	%none = %forward @ %backward @ %up @ %down !$= "" ? "" : "\c3OFF";

	%format = "<just:right><font:Consolas:18>";
	%throttle = "\c5Throttle: \c6[\c2" @ trim(%forward @ %backward @ %up @ %down @ %none) @ "\c6]";
	%hspeedometer = "\c5Horiz. Speed: \c6[\c2" @ mFloor(%horizontalSpeed * 10 + 0.5) @ " \c6|\c5" @ %maxHorizontalSpeed @ "\c6]";
	%vspeedometer = "\c5Vert. Speed: \c6[\c2" @ mFloor(%verticalSpeed * 10 + 0.5) @ " \c6|\c5" @ %maxVerticalSpeed @ "\c6]";
	%freelook = %freelook ? "\c0-FREELOOK ON-" : "";
	%rf = " <br>";
	%cl.centerprint(%format @ %throttle @ %rf @ %hspeedometer @ %rf @ %vspeedometer @ %rf @ %freelook, 1);
}

function GameConnection::centerprintPlaneControl(%cl)
{
	%plane = %cl.controllingAircraft;
	if (!isObject(%plane) || !%plane.isplane)
	{
		return;
	}

	%velocity = %plane.getVelocity();
	%speed = vectorLen(%velocity);
	%freelook 	= %cl.aircraftFreeLook;
	%maxSpeed 		= %plane.maxSpeed * 10;
	%minSpeed 		= %plane.minSpeed * 10;
	%driftFactor 			= %plane.driftFactor;
	%forward = %cl.forwardMovement && !%cl.backMovement ? "FWD++ " : "";
	%backward = %cl.backMovement && !%cl.forwardMovement ? "\c3OFF " : "";
	%none = %forward @ %backward !$= "" ? "" : "FWD";

	%format = "<just:right><font:Consolas:18>";
	%throttle = "\c5Throttle: \c6[\c2" @ trim(%forward @ %backward @ %up @ %down @ %none) @ "\c6]";
	%speedometer = "\c5Speed: \c6[\c5" @ %minSpeed @ "\c6|\c2 " @ mFloor(%speed * 10 + 0.5) @ " \c6|\c5" @ %maxSpeed @ "\c6]";
	%freelook = %freelook ? "\c0-FREELOOK ON-" : "";
	%rf = " <br>";
	%cl.centerprint(%format @ %throttle @ %rf @ %speedometer @ %rf @ %vspeedometer @ %rf @ %freelook, 1);
}

package resetCamera
{
	function GameConnection::setControlObject(%cl, %obj)
	{
		if (%cl.controllingAircraft)
		{
			%cl.camera.isSpying = 0;
			%cl.controllingAircraft = 0;
			%cl.camera.setMode("Observer");
		}
		return parent::setControlObject(%cl, %obj);
	}

	function Observer::onTrigger(%this, %obj, %trigger, %state)
	{
		%cl = %obj.getControllingClient();
		if (%cl.controllingAircraft)
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

			if (%cl.controllingAircraft.isPlane && %trigger == 2)
			{
				%cl.controllingAircraft.setImageTrigger(0, %state);
			}
		}
		parent::onTrigger(%this, %obj, %trigger, %state);
	}

	function serverCmdShiftBrick(%cl, %x, %y, %z)
	{
		if (%cl.controllingAircraft)
		{
			toggleFreeLook(%cl);
			return;
		}
		parent::serverCmdShiftBrick(%cl, %x, %y, %z);
	}

	function serverCmdRotateBrick(%cl, %rot)
	{
		if (%cl.controllingAircraft)
		{
			toggleFreeLook(%cl);
			return;
		}
		parent::serverCmdRotateBrick(%cl, %rot);
	}

	function serverCmdPlantBrick(%cl)
	{
		if (%cl.controllingAircraft)
		{
			toggleFreeLook(%cl);
			return;
		}
		parent::serverCmdPlantBrick(%cl);
	}

	function serverCmdCancelBrick(%cl)
	{
		if (%cl.controllingAircraft)
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
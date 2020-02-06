
$minBlimpSpeed = 0.1;
$blimpAcceleration = 0.2;

// function AIPlayer::moveBlimp(%blimp, %destination)
// {
// 	if (!%blimp.isBlimp)
// 	{
// 		return;
// 	}

// 	%blimp.setAimLocation(%destination);
// 	// %blimp.setMoveY(1);
// 	if (%blimp.moveSpeed $= "")
// 	{
// 		%blimp.setMoveY($minBlimpSpeed);
// 		%blimp.moveSpeed = $minBlimpSpeed;
// 	}
// 	%blimp.lastMoved = getSimTime();
// 	%blimp.distanceCheckLoop(%destination);
// 	if (isObject(%blimp.moveShape))
// 	{
// 		%blimp.moveShape.delete();
// 	}
// 	%blimp.moveShape = createRingMarker(%destination, "0 1 0 1", "1 1 0.1");
// }

// function AIPlayer::distanceCheckLoop(%blimp, %destination)
// {
// 	cancel(%blimp.distanceCheckLoop);
// 	%directVec = vectorSub(%destination, %blimp.getPosition());
// 	%dist = vectorLen(%directVec);
// 	%currVec = %blimp.getForwardVector();
// 	%dot = vectorDot(vectorNormalize(getWords(%directVec, 0, 1)), %currVec);
// 	%currMovespeed = %blimp.moveSpeed;

// 	%timeSinceLastMove = getSimTime() - %blimp.lastMoved;
// 	%accelerationAmount = (%timeSinceLastMove / 1000 * $blimpAcceleration);

// 	if (%dot > 0.8) //if we're facing the target, speed up slowly
// 	{
// 		%addedVel++;
// 		%currMovespeed = getMin(%currMovespeed + %accelerationAmount, %dot);
// 	}
// 	else //we're not facing the target, slow down
// 	{
// 		%pre = %currMovespeed;
// 		%currMovespeed = getMax(%currMovespeed - %accelerationAmount, $minBlimpSpeed);
// 		%sub = %sub + (%pre - %currMovespeed);
// 	}
	
// 	if (%dist < 4) //if we're close to the target, slow down
// 	{
// 		%slowingDown = 1;
// 		%pre = %currMovespeed;
// 		%currMovespeed = getMax((%currMovespeed - 0.1) * (1 - 0.99 * %timeSinceLastMove / 1000) + 0.1, $minBlimpSpeed);
// 		%sub = %sub + (%pre - %currMovespeed);
// 	}

// 	%blimp.setMoveY(%currMovespeed);
// 	%blimp.moveSpeed = %currMovespeed;
// 	if (%blimp.debug)
// 	{
// 		echo("Mul: " @ (1 - 0.8 * %timeSinceLastMove / 1000) @ " | Movespeed: " @ %currMovespeed @ " | Dist: " @ %dist, 8564862);
// 		%blimp.setShapeName("Movespeed: " @ %currMovespeed @ " | Dist: " @ %dist, 8564862);
// 	}

// 	if (%dist < 0.5)
// 	{
// 		%blimp.setMoveY(0);
// 		%blimp.clearAim();
// 		%blimp.moveShape.delete();
// 		%blimp.moveSpeed = "";
// 		return;
// 	}

// 	%blimp.lastMoved = getSimTime();

// 	%blimp.distanceCheckLoop = %blimp.schedule(33, distanceCheckLoop, %destination);
// }

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

	%cl.controllingBlimp = %blimp;
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
	%flatvec = vectorNormalize(getWords(%cl.camera.getEyeVector(), 0, 1));
	%blimp.setAimVector(%flatvec);
	//prevents rotation updates not being sent to client due to slow turns
	%blimp.addVelocity("0 0 0.01");
	%blimp.addVelocity("0 0 -0.01");

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

	%finalVelocity = vectorAdd(%originalVelocity, %addedVelocity);

	//clamp and fix z velocity
	//decelerate the ship if it is above max vel
	%z = getWord(%finalVelocity, 2);
	%z = mAbs(%z) > %maxVerticalSpeed ? (%z > 0 ? %z + %downAcceleration : %z + %upAcceleration) : %z;
	//clamp and fix horiz velocity
	%horizVel = getWords(%finalVelocity, 0, 1);
	%horizVelProj = vectorScale(%forwardDir, vectorDot(%horizVel, %forwardDir));
	%fixedHVector = vectorAdd(vectorScale(%horizVel, %driftFactor), vectorScale(%horizVelProj, 1 - %driftFactor));
	if (%vectorLen(%fixedHVector > %maxHorizontalSpeed))
	{
		%fixedHVectorNorm = vectorNormalize(%fixedHVector);
		%max = vectorScale(%fixedHVectorNorm, %maxHorizontalSpeed);
		%diff = vectorLen(vectorSub(%max, %fixedHVector));
		%fixedHVector = vectorAdd(%fixedHVector, vectorScale(%fixedHVectorNorm, -1 * getMin(%diff, %forwardAcceleration)));
	}

	%finalVelocity = getWords(%fixedHVector, 0, 1) SPC %z;
	%blimp.setVelocity(%finalVelocity);

	%cl.blimpControlSched = schedule(1, %blimp, blimpControlTick, %blimp, %cl);
}

package resetCamera
{
	function GameConnection::setControlObject(%cl, %obj)
	{
		%cl.camera.isSpying = 0;
		%cl.controllingBlimp = 0;
		%cl.camera.setMode("Observer");
		return parent::setControlObject(%cl, %obj);
	}

	function Observer::onTrigger(%this, %obj, %trigger, %state)
	{
		%cl = %obj.getControllingClient();
		if (%cl.controllingBlimp)
		{
			echo("Trigger: " @ %trigger);
			//trig 2 = spacebar
			//trig 3 = shift
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
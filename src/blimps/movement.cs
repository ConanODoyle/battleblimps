
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
	%timeSinceLastMove = getSimTime() - %blimp.lastMoved;
	%factor = %timeSinceLastMove / 1000;
	%name = mCeil(vectorLen(%blimp.getVelocity()) * 1000) / 1000 @ " ";

	//end control if camera is not in control mode
	if (%cl.controllingBlimp != %blimp || %cl.camera.mode !$= "Orbit" || !isObject(%cl.camera) || !isObject(%blimp))
	{
		%cl.camera.isSpying = 0;
		%cl.controllingBlimp = 0;
		%cl.camera.setMode("Observer");
		return;
	}

	%vec = vectorNormalize(getWords(%cl.camera.getEyeVector(), 0, 1));
	%blimp.setAimVector(%vec);
	//prevents rotation updates not being sent to client due to slow turns
	%blimp.addVelocity("0 0 0.01");
	%blimp.addVelocity("0 0 -0.01");

	%finalVelocity = %blimp.getVelocity();
	%forwardDir = %blimp.getForwardVector();

	// %name = "";
	if (%cl.upMovement && !%cl.downMovement)
	{
		%name = %name SPC "up";
		%finalVelocity = vectorAdd(%finalVelocity, "0 0 " @ 1 * %factor);
	}
	else if (%cl.downMovement && !%cl.upMovement)
	{
		%name = %name SPC "down";
		%finalVelocity = vectorAdd(%finalVelocity, "0 0 " @ -1 * %factor);
	}

	if (%cl.forwardMovement && !%cl.backMovement)
	{
		%name = %name SPC "fwd";
		%finalVelocity = vectorAdd(%finalVelocity, vectorScale(%forwardDir, 1 * %factor));
		if (%blimp.lastThread !$= "run")
		{
			%blimp.lastThread = "run";
			%blimp.playThread(0, run);
		}
	}
	else if (%cl.backMovement && !%cl.forwardMovement)
	{
		%name = %name SPC "bck";
		%finalVelocity = vectorAdd(%finalVelocity, vectorScale(%forwardDir, -1 * %factor));
		if (%blimp.lastThread !$= "back")
		{
			%blimp.lastThread = "back";
			%blimp.playThread(0, back);
		}
	}
	else
	{
		if (%blimp.lastThread !$= "root")
		{
			%blimp.lastThread = "root";
			%blimp.playThread(0, root);
		}
	}

	%z = getWord(%blimp.getVelocity(), 2);
	%unclampedVelocity = getWords(%blimp.getVelocity(), 0, 1);
	%lookDir = %forwardDir;
	%clamped = vectorScale(%lookDir, vectorDot(%unclampedVelocity, %lookDir));
	%driftFactor = %blimp.driftFactor;
	%finalVector = vectorAdd(vectorScale(%unclampedVelocity, %driftFactor), vectorScale(%clamped, 1 - %driftFactor));
	// %name = "cl:" @ vectorLen(%finalVector) SPC "or:" @vectorLen(%blimp.getVelocity());
	%blimp.setVelocity(vectorAdd(%finalVector, "0 0 " @ %z));

	%blimp.setShapeName(%name, 8564862);

	%blimp.lastMoved = getSimTime();

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
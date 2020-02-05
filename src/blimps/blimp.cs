datablock PlayerData(BlimpArmor : PlayerStandardArmor)
{
	shapeFile = "./blimp.dts";
	uiName = "";

	maxForwardSpeed = 4;
	maxBackwardSpeed = 2;
	maxSideSpeed = 0;

	boundingBox = vectorScale("2 2 2.8", 4);
};

datablock PlayerData(ScoutBlimpArmor : BlimpArmor)
{
	shapeFile = "./scoutblimp.dts";
	uiName = "";

	maxForwardSpeed = 5;
	maxBackwardSpeed = 3;
	maxSideSpeed = 0;

	boundingBox = vectorScale("1.5 1.5 2.8", 4);
};

datablock PlayerData(BuzzerPlaneArmor : BlimpArmor)
{
	shapeFile = "./buzzer.dts";
	uiName = "";

	maxForwardSpeed = 4;
	maxBackwardSpeed = 2;
	maxSideSpeed = 0;

	boundingBox = vectorScale("1.3 1.3 1.5", 4);
};

if (!isObject(BlimpSimSet))
{
	$BlimpSimSet = new SimSet(BlimpSimSet);
}

function getBlimp()
{
	%shape = new AIPlayer(Blimps)
	{
		dataBlock = BlimpArmor;
		isBlimp = 1;
	};
	$BlimpSimSet.add(%shape);
	return %shape;
}

function AIPlayer::moveBlimp(%blimp, %destination)
{
	if (!%blimp.isBlimp)
	{
		return;
	}

	%blimp.setAimLocation(%destination);
	%blimp.setMoveY(1);
	%blimp.distanceCheckLoop(%destination);
	if (isObject(%blimp.moveShape))
	{
		%blimp.moveShape.delete();
	}
	%blimp.moveShape = createRingMarker(%destination, "0 1 0 1", "1 1 0.1");
}

function AIPlayer::distanceCheckLoop(%blimp, %destination)
{
	cancel(%blimp.distanceCheckLoop);
	%dist = vectorDist(%blimp.getPosition(), %destination);

	if (%dist < 0.5 || (%dist < 1 && %dist > %blimp.lastDistanceCheck))
	{
		%blimp.setMoveY(0);
		%blimp.clearAim();
		%blimp.moveShape.delete();
		return;
	}

	%blimp.lastDistanceCheck = %dist;

	%blimp.distanceCheckLoop = %blimp.schedule(33, distanceCheckLoop, %destination);
}

function testAim(%pl, %bot)
{
	if (!isObject(%pl) || !isObject(%bot))
	{
		return;
	}

	cancel(%bot.testAimSched);

	%bot.setAimVector(%pl.getForwardVector());
	%bot.testAimSched = schedule(33, %bot, testAim, %pl, %bot);
}

function serverCmdGetBalloon(%cl)
{
	if (isObject(%cl.balloon))
	{
		%cl.balloon.delete();
	}

	if (!isObject(%cl.player))
	{
		return;
	}
	%cl.balloon = getBlimp();
	%cl.balloon.maxyawspeed = 0.8;
	%cl.balloon.setMoveY(0.2);
	%cl.balloon.setTransform(%cl.player.getTransform());
	%cl.balloon.setShapeName(%cl.name @ "'s Balloon", 8564862);
	%cl.balloon.setNodeColor("ALL", %cl.chestcolor);

	testAim(%cl.player, %cl.balloon);
}

function serverCmdTestMoveBalloon(%cl)
{
	if (!isObject(%cl.balloon))
	{
		return;
	}

	cancel(%cl.balloon.testAimSched);
	%cl.balloon.setMoveY(0);

	// talk("moving");
	if (isObject(%cl.player))
	{
		%pl = %cl.player;
		%start = %pl.getEyeTransform();
		%end = vectorAdd(vectorScale(%pl.getEyeVector(), 200), %start);
		%masks = $Typemasks::fxBrickObjectType | $Typemasks::StaticObjectType | $Typemasks::StaticShapeObjectType | $Typemasks::EnvironmentObjectType;
		%ray = containerRaycast(%start, %end, %masks);
		if (isObject(%hit = getWord(%ray, 0)))
		{
			%hitloc = getWords(%ray, 1, 3);
			// talk(%hitloc);
			%cl.balloon.moveBlimp(%hitloc);
		}
	}
}
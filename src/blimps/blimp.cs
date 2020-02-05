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
$minBlimpSpeed = 0.1;
$blimpAcceleration = 0.2;

function AIPlayer::moveBlimp(%blimp, %destination)
{
    if (!%blimp.isBlimp)
    {
        return;
    }

    %blimp.setAimLocation(%destination);
    // %blimp.setMoveY(1);
    if (%blimp.moveSpeed $= "")
    {
        %blimp.setMoveY($minBlimpSpeed);
        %blimp.moveSpeed = $minBlimpSpeed;
    }
    %blimp.lastMoved = getSimTime();
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
    %directVec = vectorSub(%destination, %blimp.getPosition());
    %dist = vectorLen(%directVec);
    %currVec = %blimp.getForwardVector();
    %dot = vectorDot(vectorNormalize(getWords(%directVec, 0, 1)), %currVec);
    %currMovespeed = %blimp.moveSpeed;

    %timeSinceLastMove = getSimTime() - %blimp.lastMoved;
    %accelerationAmount = (%timeSinceLastMove / 1000 * $blimpAcceleration);

    if (%dot > 0.8) //if we're facing the target, speed up slowly
    {
        %addedVel++;
        %currMovespeed = getMin(%currMovespeed + %accelerationAmount, %dot);
    }
    else //we're not facing the target, slow down
    {
        %pre = %currMovespeed;
        %currMovespeed = getMax(%currMovespeed - %accelerationAmount, $minBlimpSpeed);
        %sub = %sub + (%pre - %currMovespeed);
    }
    
    if (%dist < 4) //if we're close to the target, slow down
    {
        %slowingDown = 1;
        %pre = %currMovespeed;
        %currMovespeed = getMax((%currMovespeed - 0.1) * (1 - 0.99 * %timeSinceLastMove / 1000) + 0.1, $minBlimpSpeed);
        %sub = %sub + (%pre - %currMovespeed);
    }

    %blimp.setMoveY(%currMovespeed);
    %blimp.moveSpeed = %currMovespeed;
    echo("Mul: " @ (1 - 0.8 * %timeSinceLastMove / 1000) @ " | Movespeed: " @ %currMovespeed @ " | Dist: " @ %dist, 8564862);
    %blimp.setShapeName("Movespeed: " @ %currMovespeed @ " | Dist: " @ %dist, 8564862);

    if (%dist < 0.5)
    {
        %blimp.setMoveY(0);
        %blimp.clearAim();
        %blimp.moveShape.delete();
        %blimp.moveSpeed = "";
        return;
    }

    %blimp.lastMoved = getSimTime();

    %blimp.distanceCheckLoop = %blimp.schedule(33, distanceCheckLoop, %destination);
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
}
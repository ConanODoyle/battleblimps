datablock PlayerData(BlimpArmor : PlayerStandardArmor)
{
	shapeFile = "./blimp.dts";
	uiName = "";

	boundingBox = vectorScale("2 2 2", 4);
};

if (!isObject(BlimpSimSet))
{
	$BlimpSimSet = new SimSet(BlimpSimSet);
}

function getBlimp()
{
	%shape = new AIPlayer(Blimps)
	{
		dataBlock = BlimpArmorData;
		isBlimp = 1;
	};
	$BlimpSimSet.add(%shape);
	return %shape;
}

function BlimpArmor::moveBlimp(%blimp, %destination)
{
	if (!%blimp.isBlimp)
	{
		return;
	}

	%blimp.setAimLocation(%destination);
	%blimp.setMoveY(1);
	%blimp.distanceCheckLoop(%destination);
}

function BlimpArmor::distanceCheckLoop(%blimp, %destination)
{
	cancel(%blimp.distanceCheckLoop);
	%dist = vectorDist(%blimp.getPosition(), %destination);

	if (%dist < 0.5 || (%dist < 1 && %dist > %blimp.lastDistanceCheck))
	{
		%blimp.setMoveY(0);
		%blimp.clearAim();
		return;
	}

	%blimp.lastDistanceCheck = %dist;

	%blimp.distanceCheckLoop = %blimp.schedule(33, distanceCheckLoop, %destination);
}
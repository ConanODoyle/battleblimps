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
	%blimp = new AIPlayer(Blimps)
	{
		dataBlock = BlimpArmor;
		isBlimp = 1;
	};
	%blimp.hideNode("ALL");
	%blimp.unhideNode("balloon");
	%blimp.unhideNode("ship");
	%blimp.unhideNode("mesh");
	%blimp.unhideNode("ropes");
	%blimp.unhideNode("")
	$BlimpSimSet.add(%blimp);
	return %blimp;
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
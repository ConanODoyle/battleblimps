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

		upAcceleration = 1;
		downAcceleration = -1;
		forwardAcceleration = 1.2;
		backwardAcceleration = -0.7;
		maxHorizontalSpeed = 4;
		maxVerticalSpeed = 1;

		cameraDistance = 10;

		driftFactor = 0.5;
		maxYawSpeed = 0.8;
	};
	%blimp.hideNode("ALL");
	%blimp.unhideNode("balloon");
	%blimp.unhideNode("ship");
	%blimp.unhideNode("mesh");
	%blimp.unhideNode("ropes");
	%blimp.unhideNode("e_Sr_prop");
	%blimp.unhideNode("e_Sl_prop");
	%blimp.unhideNode("e_Sr");
	%blimp.unhideNode("e_Sl");
	$BlimpSimSet.add(%blimp);
	return %blimp;
}

function getPlane(%type)
{
	if (%type $= "")
	{
		%type = "buzzer";
	}

	switch$ (%type)
	{
		case "buzzer": %engine = "frontprop backprop backengine";
		case "biplane": %engine = "frontprop";
		case "dartplane": %engine = "backprop backengine";
		default: error("Invalid plane type '" @ %type @ "'!"); return;
	}

	%buzzer = new AIPlayer(Buzzer)
	{
		dataBlock = BuzzerPlaneArmor;
		isPlane = 1;

		// upAcceleration = 0;
		// downAcceleration = 0;
		// forwardAcceleration = 0;
		// backwardAcceleration = 0;
		// maxHorizontalSpeed = 10;
		// maxVerticalSpeed = 10;
		maxVerticalSpeed = 2;
		maxSpeed = 10;
		minSpeed = 2;
		maxUpSpeed = 3;
		eyeAcceleration = 2.5;
		passiveAcceleration = 1;

		cameraDistance = 4;

		driftFactor = 0.5;
		maxYawSpeed = 5;
	};
	%buzzer.hideNode("ALL");
	%buzzer.unhideNode(%type);
	for (%i = 0; %i < getWordCount(%engine); %i++)
	{
		%buzzer.unhideNode(getWord(%engine, %i));
	}
	$BlimpSimSet.add(%buzzer);
	return %buzzer;
}

function serverCmdGetBalloon(%cl)
{
	if (isObject(%cl.aircraft))
	{
		%cl.aircraft.delete();
	}

	if (!isObject(%cl.player))
	{
		return;
	}
	%cl.aircraft = getBlimp();
	// %cl.aircraft.setMoveY(0.2);
	%cl.aircraft.setTransform(%cl.player.getTransform());
	%cl.aircraft.setShapeName(%cl.name @ "'s Balloon", 8564862);
	%cl.aircraft.setNodeColor("ALL", %cl.chestcolor);
	%cl.controlAircraft(%cl.aircraft);
	if (isObject(%cl.player))
	{
		%pl = %cl.player;
		%pl.setScale("0.1 0.1 0.1");
		%cl.aircraft.mountObject(%pl, 5);
	}
}


function serverCmdGetPlane(%cl)
{
	if (isObject(%cl.aircraft))
	{
		%cl.aircraft.delete();
	}

	if (!isObject(%cl.player))
	{
		return;
	}
	%cl.aircraft = getPlane();
	// %cl.aircraft.setMoveY(0.2);
	%cl.aircraft.setTransform(%cl.player.getTransform());
	%cl.aircraft.setShapeName(%cl.name @ "'s Plane", 8564862);
	%cl.aircraft.setNodeColor("ALL", %cl.chestcolor);
	%cl.controlAircraft(%cl.aircraft);
}
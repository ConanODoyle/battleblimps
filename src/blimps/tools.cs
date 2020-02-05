datablock ItemData(BlimpControlItem : printGun)
{
	uiName = "BlimpControlImage";
	colorShiftColor = "1 0 0 1";
	image = BlimpControlImage;
};

datablock ShapeBaseImageData(BlimpControlImage)
{
	shapeFile = printGunImage.shapefile;
	emap = 1;

	doColorShift = 1;
	colorShiftColor = "1 0 0 1";

	armReady = 1;

	mountPoint = 0;

	stateName[0] = "Activate";
	stateTimeoutValue[0] = 0.1;
	stateTransitionOnTimeout[0]	= "Ready";

	stateName[1] = "Ready";
	stateTransitionOnTriggerDown[1] = "Fire";

	stateName[2] = "Fire";
	stateScript[2] = "onFire";
	stateTimeoutValue[2] = 0.1;
	stateTransitionOnTimeout[2] = "Refire";

	stateName[3] = "Refire";
	stateTransitionOnTriggerDown[3] = "Fire";
	stateTransitionOnTriggerUp[3] = "Ready";
};

function BlimpControlImage::onFire(%this, %obj, %slot)
{
	%cl = %obj.client;
	if (!isObject(%cl.balloon))
	{
		%cl.centerprint("You don't have a balloon!", 1);
		return;
	}

	cancel(%cl.balloon.testAimSched);
	%cl.balloon.setMoveY(0);

	if (isObject(%cl.player))
	{
		%pl = %cl.player;
		%start = %pl.getEyeTransform();
		%end = vectorAdd(vectorScale(%pl.getEyeVector(), 50), %start);
		%masks = $Typemasks::fxBrickObjectType | $Typemasks::StaticObjectType | $Typemasks::StaticShapeObjectType | $Typemasks::EnvironmentObjectType;
		%ray = containerRaycast(%start, %end, %masks);
		if (isObject(%hit = getWord(%ray, 0)))
		{
			%hitloc = getWords(%ray, 1, 3);
			if (isObject(%obj.controlLineShape))
			{
				cancel(%obj.controlLineShape.deleteSchedule);
				%obj.controlLineShape.drawLine(%obj.getMuzzlePoint(%slot), %hitloc, "0 1 0 0.2", 0.08);
			}
			else
			{
				%obj.controlLineShape = drawLine(%obj.getMuzzlePoint(%slot), %hitloc, "0 1 0 0.2", 0.08);
			}
			%obj.controlLineShape.deleteSchedule = %obj.controlLineShape.schedule(100, delete);
			%cl.balloon.moveBlimp(%hitloc);
		}
	}
}
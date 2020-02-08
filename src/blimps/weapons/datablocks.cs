datablock ItemData(ReferenceTurretItem : hammerItem)
{
	image = "";
	uiName = "";
	iconName = "";
	colorShiftColor = "1 1 1 1";
	turretItem = 1;
};

function onTurretImageMount(%image, %obj, %slot)
{
	%obj.maxYawSpeed = %image.maxYawSpeed !$= "" ? %image.maxYawSpeed : %obj.maxYawSpeed;
	%obj.maxPitchSpeed = %image.maxPitchSpeed !$= "" ? %image.maxPitchSpeed : %obj.maxPitchSpeed;

	%obj.setImageLoaded(%slot, %obj.gunLoaded[%image.getID()] > 0);
}

function onTurretImageFire(%image, %obj, %slot)
{
	%cl = %obj.getMountedObject(0).client;

	%projectile = %image.projectile;
	%spread = %image.spread;
	%shellcount = %image.shellCount;

	// %fvec = %obj.getForwardVector();
	// %fX = getWord(%fvec,0);
	// %fY = getWord(%fvec,1);

	// %evec = %obj.getEyeVector();
	// %eX = getWord(%evec,0);
	// %eY = getWord(%evec,1);
	// %eZ = getWord(%evec,2);

	// %eXY = mSqrt(%eX*%eX+%eY*%eY);

	// %aimVec = %fX*%eXY SPC %fY*%eXY SPC %eZ;

	for (%shell = 0; %shell < %shellcount; %shell++)
	{
		%vector = %obj.getEyeVector();
		%objectVelocity = %obj.getVelocity();
		%vector1 = VectorScale(%vector, %projectile.muzzleVelocity);
		%vector2 = VectorScale(%objectVelocity, %projectile.velInheritFactor);
		%velocity = VectorAdd(%vector1,%vector2);
		%x = (getRandom() - 0.5) * 10 * 3.1415926 * %spread;
		%y = (getRandom() - 0.5) * 10 * 3.1415926 * %spread;
		%z = (getRandom() - 0.5) * 10 * 3.1415926 * %spread;
		%mat = MatrixCreateFromEuler(%x @ " " @ %y @ " " @ %z);
		%velocity = MatrixMulVector(%mat, %velocity);

		%p = new (%image.projectileType)()
		{
			dataBlock = %projectile;
			initialVelocity = %velocity;
			initialPosition = %obj.getMuzzlePoint(%slot);
			sourceObject = %obj;
			sourceSlot = %slot;
			client = %cl;
		};
		MissionCleanup.add(%p);
	}

	%obj.gunLoaded[%image.getID()]--;
	if (%obj.gunLoaded[%image.getID()] <= 0)
	{
		%obj.setImageLoaded(%slot, 0);
	}
}

function onTurretImageReload(%image, %obj, %slot)
{
	%obj.gunLoaded[%image.getID()] = 1;
	%obj.setImageLoaded(%slot, 1);
}

forceRequiredAddon("Weapon_Gun");
forceRequiredAddon("Vehicle_Tank");
exec("./gatling.cs");
exec("./cannons.cs");
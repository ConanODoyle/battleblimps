exec("./weapons/datablocks.cs");

datablock PlayerData(TurretBaseArmor : PlayerStandardArmor)
{
	shapeFile = "./weapons/gunmount.dts";
	uiName = "";

	maxForwardSpeed = 0;
	maxBackwardSpeed = 0;
	maxSideSpeed = 0;

	keepOnDeath = 1;

	boundingBox = vectorScale("20 20 20", 4);
};

function getTurret(%weapons)
{
	%turret = new AIPlayer(Turrets)
	{
		dataBlock = TurretBaseArmor;
		weaponCount = 0;
		isTurret = 1;
	};
	%turret.kill();

	%turret.updateWeaponTable(%weapons);

	return %turret;
}

function AIPlayer::updateWeaponTable(%turret, %weapons)
{
	if (!isObject(%turret) || %turret.getDataBlock() != TurretBaseArmor.getID())
	{
		return;
	}

	%turret.weaponCount = 0;
	for (%i = 0; %i < getWordCount(%weapons); %i++)
	{
		%item = getWord(%Weapons, %i);
		if (%item.turretItem && %item.getClassName() $= "ItemData")
		{
			%turret.weapon[%turret.weaponCount] = %item;
			%turret.weaponCount++;
		}
	}

	if (isObject(%turret.weapon0.image))
	{
		%turret.mountImage(%turret.weapon0.image, 0);
	}

	if (isObject(%turret.getControllingClient()))
	{
		// sendWeaponTable(%turret.getControllingClient(), %turret);
	}
}

function GameConnection::controlTurret(%cl, %turret)
{
	// if (!%cl.controllingTurret || %cl.camera.isSpying != %turret)
	// {
	// 	%cl.setControlObject(%cl.camera); //must be first - pkg resetCamera resets subsequent lines

	// 	%cl.camera.setControlObject(%cl.camera);
	// 	%dist = %turret.cameraDistance > 0 ? %turret.cameraDistance : 10;
	// 	%cl.camera.schedule(1, setOrbitMode, %turret, %turret.getTransform(), 0, %dist, %dist, 1);
	// 	%cl.camera.isSpying = %turret;
	// 	%cl.camera.mode = "Orbit";
	// }
	%cl.setControlObject(%turret);
	//sendWeaponTable(%cl, %turret);
}


package BattleBlimps_KeepOnDeath
{
	function Armor::onDisabled(%this, %obj, %state)
	{
		if (%this.keepOnDeath)
		{
			return;
		}

		return parent::onDisabled(%this, %obj);
	}

	function Armor::onMount(%this, %obj, %vehicle, %node)
	{
		if (%node == 0.0)
		{
			if (%vehicle.isHoleBot)
			{
				if (%vehicle.controlOnMount)
				{
					%obj.setControlObject(%vehicle);
					%vehicle.lastDrivingClient = %obj.client;
				}
			}
			else
			{
				if (%vehicle.getControllingClient() == 0.0)
				{
					%obj.setControlObject(%vehicle);
					%vehicle.lastDrivingClient = %obj.client;
				}
			}
		}
		else
		{
			%obj.setControlObject(%obj);
		}
		%obj.setTransform("0 0 0 0 0 1 0");
		%obj.playThread(0, %vehicle.getDataBlock().mountThread[%node]);
		ServerPlay3D(playerMountSound, %obj.getPosition());
		if (%vehicle.getDataBlock().lookUpLimit !$= "")
		{
			%obj.setLookLimits(%vehicle.getDataBlock().lookUpLimit, %vehicle.getDataBlock().lookDownLimit);
		}
	}


};
activatePackage(BattleBlimps_KeepOnDeath);
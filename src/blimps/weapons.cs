exec("./weapons/datablocks.cs");

datablock PlayerData(TurretBaseArmor : PlayerStandardArmor)
{
	shapeFile = "./weapons/gunmount.dts";
	uiName = "";

	maxForwardSpeed = 0;
	maxBackwardSpeed = 0;
	maxSideSpeed = 0;

	keepOnDeath = 1;

	boundingBox = vectorScale("0.25 0.25 0.25", 4);
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
}

function getClampedAimVector(%turret, %target)
{
	%correctVec = correctMountedAimVector(%turret, %target, 1);
	if (!isObject(%turret.getObjectMount()))
	{
		return %correctVec;
	}
	%xy = getWords(%correctVec, 0, 1);
	%z = getWord(%correctVec, 2);
	%baseVec = %turret.getBaseVector();

	if (mACos(vectorDot(vectorNormalize(%xy), %baseVec)) > %turret.maxSideAngle)
	{
		%left = vectorCross("0 0 1", vectorNormalize(%xy));
		%proj = vectorDot(%left, %xy);
		if (%proj > 0) //on left
		{
			%newxy = vectorScale(bb_vectorRotate(vectorNormalize(getWords(%baseVec, 0, 1)), "0 0 1", %turret.maxSideAngle), vectorLen(%xy));
		}
		else //on right
		{
			%newxy = vectorScale(bb_vectorRotate(vectorNormalize(getWords(%baseVec, 0, 1)), "0 0 1", -1 * %turret.maxSideAngle), vectorLen(%xy));
		}
	}
}

package BattleBlimps_KeepOnDeath
{
	function Player::removeBody(%obj)
	{
		if (%obj.getDatablock().keepOnDeath)
		{
			return;
		}

		return parent::removeBody(%obj);
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
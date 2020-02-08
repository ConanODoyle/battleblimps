

datablock ParticleData(TurretcannonBallTrailParticle)
{
	dragCoefficient		= 3.0;
	windCoefficient		= 0.0;
	gravityCoefficient	= 0.0;
	inheritedVelFactor	= 0.0;
	constantAcceleration	= 0.0;
	lifetimeMS		= 350;
	lifetimeVarianceMS	= 0;
	spinSpeed		= 10.0;
	spinRandomMin		= -50.0;
	spinRandomMax		= 50.0;
	useInvAlpha		= true;
	animateTexture		= false;
	//framesPerSec		= 1;

	textureName		= "base/data/particles/dot";
	//animTexName		= "~/data/particles/dot";

	// Interpolation variables
	colors[0]	= "0.2 0.2 0.2 0.1";
	colors[1]	= "0.2 0.2 0.2 0.0";
	sizes[0]	= 0.8;
	sizes[1]	= 0.0;
	times[0]	= 0.0;
	times[1]	= 1.0;
};

datablock ParticleEmitterData(TurretcannonBallTrailEmitter)
{
   ejectionPeriodMS = 5;
   periodVarianceMS = 0;

   ejectionVelocity = 0; //0.25;
   velocityVariance = 0; //0.10;

   ejectionOffset = 0;

   thetaMin         = 0.0;
   thetaMax         = 90.0;  

   particles = TurretcannonBallTrailParticle;

   useEmitterColors = true;
   uiName = "";
};


datablock ParticleData(TurretCannonSmokeParticle)
{
	dragCoefficient      = 5;
	gravityCoefficient   = -1;
	inheritedVelFactor   = 0.6;
	constantAcceleration = 0.0;
	lifetimeMS           = 1200;
	lifetimeVarianceMS   = 600;
	textureName          = "base/data/particles/cloud";
	spinSpeed		= 0.0;
	spinRandomMin		= -50.0;
	spinRandomMax		= 50.0;
	colors[0]     = "0.5 0.5 0.5 0.2";
	colors[1]     = "0.5 0.5 0.5 0.0";
	sizes[0]      = 0.6;
	sizes[1]      = 1;

	useInvAlpha = true;
};

datablock ParticleEmitterData(TurretCannonSmokeEmitter)
{
   ejectionPeriodMS = 8;
   periodVarianceMS = 4;
   ejectionVelocity = 6;
   velocityVariance = 5;
   ejectionOffset   = 0;
   thetaMin         = 0;
   thetaMax         = 50;
   phiReferenceVel  = 0;
   phiVariance      = 360;
   overrideAdvance = false;
   particles = "TurretCannonSmokeParticle";

   uiName = "";
};


datablock ExplosionData(TurretCannonBallExplosion)
{
   lifeTimeMS = 150;

   debris = tankShellDebris;
   debrisNum = 30;
   debrisNumVariance = 10;
   debrisPhiMin = 0;
   debrisPhiMax = 360;
   debrisThetaMin = 0;
   debrisThetaMax = 180;
   debrisVelocity = 80;
   debrisVelocityVariance = 30;

   particleEmitter = gravityRocketExplosionEmitter;
   particleDensity = 4;
   particleRadius = 0.1;

   emitter[0] = gravityRocketExplosionRingEmitter;
   emitter[1] = gravityRocketExplosionChunkEmitter;

   faceViewer     = true;
   explosionScale = "1 1 1";

   shakeCamera = true;
   camShakeFreq = "8.0 9.0 8.0";
   camShakeAmp = "2.0 8.0 2.0";
   camShakeDuration = 0.5;
   camShakeRadius = 5.0;

   // Dynamic light
   lightStartRadius = 5;
   lightEndRadius = 20;
   lightStartColor = "1 1 0 1";
   lightEndColor = "1 0 0 0";

   damageRadius = 3;
   radiusDamage = 5;

   impulseRadius = 4;
   impulseForce = 2000;

   playerBurnTime = 0;
};

if ($DamageType::CannonBallDirect !$= "")
{
	AddDamageType("CannonBallDirect",   '<bitmap:add-ons/Vehicle_Pirate_Cannon/ball> %1',       '%2 <bitmap:add-ons/Vehicle_Pirate_Cannon/ball> %1',       1, 1);
	AddDamageType("CannonBallRadius",   '<bitmap:add-ons/Vehicle_Pirate_Cannon/ballRadius> %1', '%2 <bitmap:add-ons/Vehicle_Pirate_Cannon/ballRadius> %1', 1, 0);
}
datablock ProjectileData(TurretCannonBallProjectile)
{
   projectileShapeName = "./CannonBall.dts";
   directDamage        = 20;
   directDamageType = $DamageType::CannonBallDirect;
   radiusDamageType = $DamageType::CannonBallRadius;
   impactImpulse	   = 1000;
   verticalImpulse	   = 0;
   explosion           = TurretCannonBallExplosion;
   particleEmitter     = TurretCannonBallTrailEmitter;

   brickExplosionRadius = 0;
   brickExplosionImpact = true;          //destroy a brick if we hit it directly?
   brickExplosionForce  = 0;             
   brickExplosionMaxVolume = 0;          //max volume of bricks that we can destroy
   brickExplosionMaxVolumeFloating = 0;  //max volume of bricks that we can destroy if they aren't connected to the ground (should always be >= brickExplosionMaxVolume)

   sound = WhistleLoopSound;

   muzzleVelocity      = 50;
   velInheritFactor    = 1;

   armingDelay         = 0;
   lifetime            = 10000;
   fadeDelay           = 10000;
   bounceElasticity    = 0.5;
   bounceFriction      = 0.20;
   isBallistic         = true;
   gravityMod = 1.0;

   hasLight    = false;
   lightRadius = 5.0;
   lightColor  = "1 0.5 0.0";

   explodeOnDeath = 1;

   uiName = "Turret Cannon Ball"; //naming it this way because it's a cannon ball
};

datablock ItemData(SingleCannonTurretItem : ReferenceTurretItem)
{
	shapeFile = "./singlecannon.dts";
	uiName = "SingleCannon";
	image = "SingleCannonTurretImage";
};

datablock ShapeBaseImageData(SingleCannonTurretImage)
{
	item = SingleCannonTurretItem;
	shapeFile = "./singlecannon.dts";
	colorShiftColor = "1 1 1 1";

	projectile = CannonProjectile;
	projectileType = Projectile;
	spread = 0.0003;
	shellcount = 1;
	chargeMax = 3000;
	minCharge = 0.1;

	mountPoint = 0;

	stateName[0] = "Activate";
	stateTimeoutValue[0] = 0.1;
	stateTransitionOnTimeout[0] = "AmmoCheck";

	stateName[1] = "AmmoCheck";
	stateTransitionOnLoaded[1] = "Ready";
	stateTransitionOnNotLoaded[1] = "Reload";

	stateName[2] = "Ready";
	stateScript[2] = "onReady";
	stateTransitionOnTriggerDown[2] = "Charge";

	stateName[3] = "Charge";
	stateTransitionOnTriggerUp[3] = "Fire";

	stateName[4] = "Fire";
	stateScript[4] = "onFire";
	stateSound[4] = "TankShotSound";
	stateEmitter[4] = TurretCannonSmokeEmitter;
	stateEmitterNode[4] = "muzzlePoint";
	stateEmitterTime[4] = 0.1;
	stateTimeoutValue[4] = 0.2;
	stateTransitionOnTimeout[4] = "Reload";

	stateName[5] = "Reload";
	stateTransitionOnTimeout[5] = "FinishReload";
	stateEmitter[5] = GatlingSmokeEmitter;
	stateEmitterNode[5] = "muzzlePoint";
	stateEmitterTime[5] = 5;
	stateTimeoutValue[5] = 5;

	stateName[6] = "FinishReload";
	stateScript[6] = "onFinishReload";
	stateTransitionOnLoaded[6] = "AmmoCheck";
};

function SingleCannonTurretImage::onMount(%this, %obj, %slot)
{	
	onTurretImageMount(%this, %obj, %slot);
}

function SingleCannonTurretImage::onCharge(%this, %obj, %slot)
{
	%obj.chargeStartTime = getSimTime();

	if (isObject(%cl = %obj.client) || isObject(%cl = %obj.getControllingClient()))
	{
		centerprintChargeLoop(%cl, %obj, %this, %slot);
	}
}

function SingleCannonTurretImage::onFire(%this, %obj, %slot)
{
	if (!isObject(%obj.client) && !isObject(%obj.getControllingClient()))
	{
		%obj.unmountImage(%slot);
		%obj.mountImage(%this, %slot);
	}
	%obj.chargeAmount = getMax(%this.minCharge, getMin((getSimTime() | 0 - %obj.chargeStartTime | 0) / %this.chargeMax, 1));
	onCannonTurretImageFire(%this, %obj, %slot);
}

function SingleCannonTurretImage::onFinishReload(%this, %obj, %slot)
{	
	onTurretImageReload(%this, %obj, %slot);
	%obj.gunLoaded[%this.getID()] = 20;
}

function onCannonTurretImageFire(%image, %obj, %slot)
{
	%cl = %obj.getMountedObject(0).client;

	%projectile = %image.projectile;
	%spread = %image.spread;
	%shellcount = %image.shellCount;

	for (%shell = 0; %shell < %shellcount; %shell++)
	{
		%vector = %obj.getEyeVector();
		%objectVelocity = %obj.getVelocity();
		%vector1 = VectorScale(%vector, %projectile.muzzleVelocity * %obj.chargeAmount);
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

function centerprintChargeLoop(%cl, %obj, %image, %slot)
{
	cancel(%cl.centerprintChargeLoop)

	if ((%obj.getControllingClient() != %cl && %obj.client != %cl) || !isObject(%obj) || %obj.getMountedImage(%slot) != %image)
	{
		return;
	}

	%format = "<just:right><font:Consolas:22>\c5";
	%amt = getMax(%image.minCharge, getMin((getSimTime() | 0 - %obj.chargeStartTime | 0) / %image.chargeMax, 1));
	%velocity = %image.projectile.muzzleVelocity * %amt;
	%rf = " <br>";
	%cl.centerprint(%format @ "Charge: \c6[\c2" @ mFloor(%amt * 1000 + 0.5) / 10 @ "%\c6]" @ %rf @ "\c5Speed: \c6[\c2" @ %velocity @ " \c6|\c5" @ %image.projectile.muzzleVelocity @ "\c6]");
	
	%cl.centerprintChargeLoop = schedule(1, %cl, centerprintChargeLoop, %cl, %obj, %image, %slot);
}


//gatling


//bullet trail effects
datablock ParticleData(GatlingTrailParticle)
{
	dragCoefficient      = 3;
	gravityCoefficient   = -0.0;
	inheritedVelFactor   = 0.5;
	constantAcceleration = 0.0;
	lifetimeMS           = 200;
	lifetimeVarianceMS   = 0;
	textureName          = "base/data/particles/dot";
	spinSpeed		= 10.0;
	spinRandomMin		= -500.0;
	spinRandomMax		= 500.0;
	colors[0]     = "1 1 0 1";
	colors[1]     = "1 1 0 0.8";
	colors[2]     = "1 1 0 0";
	sizes[0]      = 0.06;
	sizes[1]      = 0.06;
	sizes[2]	  = 0.04;
	times[0]      = 0.0;
	times[1]	  = 0.3;
	times[2]      = 1.0;

	useInvAlpha = false;
};

datablock ParticleEmitterData(GatlingTrailEmitter)
{
   ejectionPeriodMS = 3;
   periodVarianceMS = 0;
   ejectionVelocity = 0.0;
   velocityVariance = 0.0;
   ejectionOffset   = 0.0;
   thetaMin         = 0;
   thetaMax         = 90;
   phiReferenceVel  = 0;
   phiVariance      = 360;
   overrideAdvance = false;
   particles = "GatlingTrailParticle";
};

datablock ProjectileData(GatlingProjectile)
{
   projectileShapeName = "./empty.dts";
   directDamage        = 1;
   directDamageType    = $DamageType::Default;
   radiusDamageType    = $DamageType::Default;

   brickExplosionRadius = 0;
   brickExplosionImpact = true;          //destroy a brick if we hit it directly?
   brickExplosionForce  = 0;
   brickExplosionMaxVolume = 0;          //max volume of bricks that we can destroy
   brickExplosionMaxVolumeFloating = 0;  //max volume of bricks that we can destroy if they aren't connected to the ground

   impactImpulse	     = 100;
   verticalImpulse	  = 0;
   explosion           = GunExplosion;
   particleEmitter     = GatlingTrailEmitter;

   muzzleVelocity      = 30;
   velInheritFactor    = 1;

   armingDelay         = 00;
   lifetime            = 6000;
   fadeDelay           = 5500;
   bounceElasticity    = 0.5;
   bounceFriction      = 0.20;
   isBallistic         = false;
   gravityMod = 0.0;

   hasLight    = false;
   lightRadius = 3.0;
   lightColor  = "0 0 0.5";
};

//firing smoke
datablock ParticleData(GatlingSmokeParticle)
{
	dragCoefficient      = 3;
	gravityCoefficient   = -0.5;
	inheritedVelFactor   = 1;
	constantAcceleration = 0.0;
	lifetimeMS           = 525;
	lifetimeVarianceMS   = 55;
	textureName          = "base/data/particles/cloud";
	spinSpeed		= 10.0;
	spinRandomMin		= -500.0;
	spinRandomMax		= 500.0;
	colors[0]     = "0.5 0.5 0.5 0.9";
	colors[1]     = "0.5 0.5 0.5 0.0";
	sizes[0]      = 0.1;
	sizes[1]      = 0.1;

	useInvAlpha = false;
};

datablock ParticleEmitterData(GatlingSmokeEmitter)
{
   ejectionPeriodMS = 3;
   periodVarianceMS = 0;
   ejectionVelocity = 0.5;
   velocityVariance = 0.1;
   ejectionOffset   = 0.0;
   thetaMin         = 0;
   thetaMax         = 180;
   phiReferenceVel  = 0;
   phiVariance      = 360;
   overrideAdvance = false;
   particles = "GatlingSmokeParticle";

   uiName = "Gun Smoke";
};

datablock ItemData(GatlingTurretItem : ReferenceTurretItem)
{
	shapeFile = "./minigun.dts";
	uiName = "Gatling";
	image = "GatlingTurretImage";
};

datablock ShapeBaseImageData(GatlingTurretImage)
{
	item = GatlingTurretItem;
	shapeFile = "./minigun.dts";
	colorShiftColor = "1 1 1 1";

	projectile = GatlingProjectile;
	projectileType = Projectile;
	spread = 0.001;
	shellcount = 1;

	mountPoint = 0;

	stateName[0] = "Activate";
	stateTimeoutValue[0] = 0.2;
	stateSequence[0] = "root";
	stateTransitionOnTimeout[0] = "AmmoCheck";

	stateName[1] = "AmmoCheck";
	stateTransitionOnLoaded[1] = "Ready";
	stateTransitionOnNotLoaded[1] = "Reload";

	stateName[2] = "Ready";
	stateScript[2] = "onReady";
	stateSequence[2] = "root";
	stateTimeoutValue[2] = 0.1;
	stateTransitionOnTriggerDown[2] = "Fire";

	stateName[3] = "Fire";
	stateTimeoutValue[3] = 0.1;
	stateScript[3] = "onFire";
	stateEmitter[3] = GatlingSmokeEmitter;
	stateEmitterNode[3] = "muzzlePoint";
	stateEmitterTime[3] = 0.1;
	stateSequence[3] = "spin";
	stateSound[3] = "gunShot1Sound";
	stateTransitionOnTimeout[3] = "AmmoCheck";

	stateName[4] = "Reload";
	stateTransitionOnTimeout[4] = "FinishReload";
	stateEmitter[4] = GatlingSmokeEmitter;
	stateEmitterNode[4] = "muzzlePoint";
	stateEmitterTime[4] = 5;
	stateTimeoutValue[4] = 5;

	stateName[5] = "FinishReload";
	stateScript[5] = "onFinishReload";
	stateTransitionOnLoaded[5] = "AmmoCheck";
};

function GatlingTurretImage::onMount(%this, %obj, %slot)
{	
	onTurretImageMount(%this, %obj, %slot);
}

function GatlingTurretImage::onReady(%this, %obj, %slot)
{	
}

function GatlingTurretImage::onFire(%this, %obj, %slot)
{	
	onTurretImageFire(%this, %obj, %slot);
}

function GatlingTurretImage::onFinishReload(%this, %obj, %slot)
{	
	onTurretImageReload(%this, %obj, %slot);
	%obj.gunLoaded[%this.getID()] = 20;
}

datablock ItemData(ReferenceTurretItem : hammerItem)
{
	image = "";
	uiName = "";
	iconName = "";
	colorShiftColor = "1 1 1 1";
	turretItem = 1;
};





//gatling


//bullet trail effects
datablock ParticleData(GatlingTrailParticle)
{
	dragCoefficient      = 3;
	gravityCoefficient   = -0.0;
	inheritedVelFactor   = 1.0;
	constantAcceleration = 0.0;
	lifetimeMS           = 525;
	lifetimeVarianceMS   = 55;
	textureName          = "base/data/particles/dot";
	spinSpeed		= 10.0;
	spinRandomMin		= -500.0;
	spinRandomMax		= 500.0;
	colors[0]     = "0.9 0.0 0.0 0.8";
	colors[1]     = "0.9 0.3 0.0 0.6";
	colors[2]     = "0.9 0.3 0.0 0.0";
	sizes[0]      = 0.06;
	sizes[1]      = 0.06;
	sizes[2]	  = 0.01;
	times[0]      = 0.0;
	times[1]	  = 0.9;
	times[2]      = 1.0;

	useInvAlpha = false;
};

datablock ParticleEmitterData(GatlingTrailEmitter)
{
   ejectionPeriodMS = 8;
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
   directDamage        = 15;
   directDamageType    = $DamageType::Default;
   radiusDamageType    = $DamageType::Default;

   brickExplosionRadius = 0;
   brickExplosionImpact = true;          //destroy a brick if we hit it directly?
   brickExplosionForce  = 10;
   brickExplosionMaxVolume = 15;          //max volume of bricks that we can destroy
   brickExplosionMaxVolumeFloating = 20;  //max volume of bricks that we can destroy if they aren't connected to the ground

   impactImpulse	     = 400;
   verticalImpulse	  = 300;
   explosion           = GunExplosion;
   particleEmitter     = GatlingTrailEmitter;

   muzzleVelocity      = 180;
   velInheritFactor    = 1;

   armingDelay         = 00;
   lifetime            = 4000;
   fadeDelay           = 3500;
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

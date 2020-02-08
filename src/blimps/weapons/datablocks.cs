datablock ItemData(ReferenceTurretItem : hammerItem)
{
	image = "";
	uiName = "";
	iconName = "";
	colorShiftColor = "1 1 1 1";
	turretItem = 1;
};





//gatling

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
   thetaMax         = 360;
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
	image = "GatlingTurretImageAAAAA";
};

datablock ShapeBaseImageData(GatlingTurretImageAAAAA)
{
	item = GatlingTurretItem;
	shapeFile = "./minigun.dts";
	colorShiftColor = "1 1 1 1";

	projectile = gunProjectile;
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

function GatlingTurretImageAAAAA::onMount(%this, %obj, %slot)
{	
	onTurretImageMount(%this, %obj, %slot);
}

function GatlingTurretImageAAAAA::onReady(%this, %obj, %slot)
{	
}

function GatlingTurretImageAAAAA::onFire(%this, %obj, %slot)
{	
	onTurretImageFire(%this, %obj, %slot);
}

function GatlingTurretImageAAAAA::onFinishReload(%this, %obj, %slot)
{	
	onTurretImageReload(%this, %obj, %slot);
	%obj.gunLoaded[%this.getID()] = 20;
}

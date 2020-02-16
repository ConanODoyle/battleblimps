function bottomprintImageState(%cl, %obj, %slot)
{
	cancel(%cl.bottomprintImageStateSched);
	%cl.bottomprint(%obj.getImageState(%slot), 1);
	%cl.bottomprintImageStateSched = schedule(1, %obj, bottomprintImageState, %cl, %obj, %slot);
}

function correctMountedAimVector(%turret, %target, %flat)
{
	if (strLen(%target) == 1 && isObject(%target))
	{
		%targetPos = %target.getPosition();
	}
	else if (strLen(%target) >= 3)
	{
		%targetPos = %target;
	}

	if (!isObject(%turret.getObjectMount()))
	{
		return vectorNormalize(vectorSub(%targetPos, %turret.getEyeTransform()));
	}
	%tEyePos = %turret.getEyeTransform();
	%mount = %turret.getObjectMount();
	%pEyeVec = %mount.getEyeVector();
	if (%flat)
	{
		%pEyeVec = %mount.getForwardVector();
	}

	%trueAimVec = vectorNormalize(vectorSub(%targetPos, %tEyePos));

	%upVec = vectorCross(%pEyeVec, "0 1 0");
	%rot = mACos(vectorDot(%pEyeVec, "0 1 0"));

	%fixedAimVec = bb_vectorRotate(%trueAimVec, %upVec, %rot);

	return %fixedAimVec;
}

function t1()
{
	$t.setaimvector(vecToTarget($t, $target, 1));
}

function t2()
{
	$t.setaimvector(correctMountedAimVector($t, $target, 1));
}

function t3()
{
	$t.setaimvector(correctMountedAimVector($t, $target, 0));
}

function bb_vectorRotate(%vec, %axis, %angle)
{
	if (vectorLen(%axis) != 1)
	{
		%axis = vectorNormalize(%axis);
	}

	%proj = vectorScale(%axis, vectorDot(%vec, %axis));
	%ortho = vectorSub(%vec, %proj);
	%w = vectorCross(%axis, %ortho);
	%cos = mCos(%angle);
	%sin = mSin(%angle);
	%x1 = %cos / vectorLen(%ortho);
	%x2 = %sin / vectorLen(%w);
	%rotOrtho = vectorScale(vectorAdd(vectorScale(%ortho, %x1), vectorScale(%w, %x2)), vectorLen(%ortho));
	return vectorAdd(%rotOrtho, %proj);
}

//MatrixMulVector(MatrixCreate("0 0 0", vectorNormalize(%axis) SPC -1 * %deg), %vec);

function drawArrow(%pos, %vec, %color, %length, %scale, %offset)
{
	%shape = getShapelineShape("arrow");
	%shape.isLine = 1;
	%pos1 = %pos;
	%pos2 = vectorAdd(vectorScale(%vec, %length), %pos);
	%shape.drawLine(%pos1, %pos2, %color, %scale, %offset);
	return %shape;
}

function drawArrow2(%start, %end, %color, %scale, %offset)
{
	%shape = getShapelineShape("arrow");
	%shape.isLine = 1;
	%pos1 = %pos;
	%pos2 = vectorAdd(vectorScale(%vec, %length), %pos);
	%shape.drawLine(%pos1, %pos2, %color, %scale, %offset);
	return %shape;
}
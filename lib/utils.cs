function bottomprintImageState(%cl, %obj, %slot)
{
	cancel(%cl.bottomprintImageStateSched);
	%cl.bottomprint(%obj.getImageState(%slot), 1);
	%cl.bottomprintImageStateSched = schedule(1, %obj, bottomprintImageState, %cl, %obj, %slot);
}
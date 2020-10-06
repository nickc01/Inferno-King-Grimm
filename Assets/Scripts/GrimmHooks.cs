using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
	public abstract class GrimmHooks
	{
		public virtual void OnGrimmAwake(InfernoKingGrimm grimm) { }
		public virtual void OnGrimmBattleBegin(InfernoKingGrimm grimm) { }
		public virtual void OnGrimmBattleEnd(InfernoKingGrimm grimm) { }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class Record
{
	public string BossName;
	public long Score;
	public string Date;
}

[Serializable]
public class InfiniteGrimmRecords
{
	public List<Record> Records = new List<Record>();
}


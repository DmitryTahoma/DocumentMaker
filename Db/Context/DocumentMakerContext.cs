using Db.Context.ActPart;
using Db.Context.BackPart;
using Db.Context.HumanPart;
using System.Data.Entity;

namespace Db.Context
{
	public class DocumentMakerContext : DbContext
	{
		public DocumentMakerContext() : base("Data Source=10.32.16.170,1433;Network Library=DBMSSOCN;Initial Catalog=DocumentMaker;User ID=ProgTest; Password=qwerty123;") { }

		#region ActPart

		public DbSet<Act> Acts { get; set; }
		public DbSet<ActPart.ActPart> ActParts { get; set; }
		public DbSet<FullAct> FullActs { get; set; }
		public DbSet<FullWork> FullWorks { get; set; }
		public DbSet<Regions> Regions { get; set; }
		public DbSet<TemplateType> TemplateTypes { get; set; }
		public DbSet<Work> Works { get; set; }
		public DbSet<WorkBackAdapter> WorkBackAdapters { get; set; }
		public DbSet<WorkType> WorkTypes { get; set; }

		#endregion

		#region BackPart

		public DbSet<AlternativeProjectName> AlternativeProjectNames { get; set; }
		public DbSet<Back> Backs { get; set; }
		public DbSet<BackType> BackTypes { get; set; }
		public DbSet<CountRegions> CountRegions { get; set; }
		public DbSet<Episode> Episodes { get; set; }
		public DbSet<Minigame> Minigames { get; set; }
		public DbSet<Project> Projects { get; set; }

		#endregion

		#region HumanPart

		public DbSet<Address> Addresses { get; set; }
		public DbSet<Bank> Banks { get; set; }
		public DbSet<Contract> Contracts { get; set; }
		public DbSet<Human> Humans { get; set; }
		public DbSet<LocalityType> LocalityTypes { get; set; }
		public DbSet<StreetType> StreetTypes { get; set; }

		#endregion
	}
}

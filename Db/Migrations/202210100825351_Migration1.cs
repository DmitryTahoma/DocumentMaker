namespace Db.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Migration1 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Minigames", "BackId", "dbo.Backs");
            DropIndex("dbo.Minigames", new[] { "BackId" });
            AddColumn("dbo.WorkBackAdapters", "AlternativeProjectNameId", c => c.Int());
            AddColumn("dbo.Backs", "Number", c => c.Single(nullable: false));
            CreateIndex("dbo.WorkBackAdapters", "AlternativeProjectNameId");
            AddForeignKey("dbo.WorkBackAdapters", "AlternativeProjectNameId", "dbo.AlternativeProjectNames", "Id");
            DropTable("dbo.Minigames");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Minigames",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BackId = c.Int(),
                        Number = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            DropForeignKey("dbo.WorkBackAdapters", "AlternativeProjectNameId", "dbo.AlternativeProjectNames");
            DropIndex("dbo.WorkBackAdapters", new[] { "AlternativeProjectNameId" });
            DropColumn("dbo.Backs", "Number");
            DropColumn("dbo.WorkBackAdapters", "AlternativeProjectNameId");
            CreateIndex("dbo.Minigames", "BackId");
            AddForeignKey("dbo.Minigames", "BackId", "dbo.Backs", "Id");
        }
    }
}

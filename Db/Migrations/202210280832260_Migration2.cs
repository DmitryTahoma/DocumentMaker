namespace Db.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Migration2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.WorkTypes", "ActPartId", c => c.Int());
            CreateIndex("dbo.WorkTypes", "ActPartId");
            AddForeignKey("dbo.WorkTypes", "ActPartId", "dbo.ActParts", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.WorkTypes", "ActPartId", "dbo.ActParts");
            DropIndex("dbo.WorkTypes", new[] { "ActPartId" });
            DropColumn("dbo.WorkTypes", "ActPartId");
        }
    }
}

namespace Db.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ActParts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Acts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        HumanId = c.Int(),
                        TemplateTypeId = c.Int(),
                        CreationTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Humen", t => t.HumanId)
                .ForeignKey("dbo.TemplateTypes", t => t.TemplateTypeId)
                .Index(t => t.HumanId)
                .Index(t => t.TemplateTypeId);
            
            CreateTable(
                "dbo.FullActs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ActId = c.Int(),
                        TechnicalTaskDate = c.DateTime(nullable: false),
                        ActDate = c.DateTime(nullable: false),
                        TechnicalTaskNumber = c.Int(nullable: false),
                        ActSum = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Acts", t => t.ActId)
                .Index(t => t.ActId);
            
            CreateTable(
                "dbo.Humen",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Surname = c.String(),
                        Secondname = c.String(),
                        TIN = c.Long(nullable: false),
                        BankId = c.Int(),
                        CheckingAccount = c.String(),
                        DevelopmentContractId = c.Int(),
                        SupportContractId = c.Int(),
                        AddressId = c.Int(),
                        IsFired = c.Boolean(nullable: false),
                        FiredDate = c.DateTime(storeType: "date"),
                        EmploymentDate = c.DateTime(storeType: "date"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Addresses", t => t.AddressId)
                .ForeignKey("dbo.Banks", t => t.BankId)
                .ForeignKey("dbo.Contracts", t => t.DevelopmentContractId)
                .ForeignKey("dbo.Contracts", t => t.SupportContractId)
                .Index(t => t.BankId)
                .Index(t => t.DevelopmentContractId)
                .Index(t => t.SupportContractId)
                .Index(t => t.AddressId);
            
            CreateTable(
                "dbo.Addresses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LocalityTypeId = c.Int(),
                        LocalityName = c.String(),
                        StreetTypeId = c.Int(),
                        StreetName = c.String(),
                        HouseNumber = c.String(),
                        ApartmentNumber = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.LocalityTypes", t => t.LocalityTypeId)
                .ForeignKey("dbo.StreetTypes", t => t.StreetTypeId)
                .Index(t => t.LocalityTypeId)
                .Index(t => t.StreetTypeId);
            
            CreateTable(
                "dbo.LocalityTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ShortName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.StreetTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ShortName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Banks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        IBT = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Contracts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Number = c.String(),
                        PreparationDate = c.DateTime(nullable: false, storeType: "date"),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TemplateTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.WorkTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TemplateTypeId = c.Int(),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TemplateTypes", t => t.TemplateTypeId)
                .Index(t => t.TemplateTypeId);
            
            CreateTable(
                "dbo.Works",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ActId = c.Int(),
                        WorkTypeId = c.Int(),
                        ActPartId = c.Int(),
                        WorkBackAdapterId = c.Int(),
                        SpentTime = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ActParts", t => t.ActPartId)
                .ForeignKey("dbo.WorkBackAdapters", t => t.WorkBackAdapterId)
                .ForeignKey("dbo.WorkTypes", t => t.WorkTypeId)
                .ForeignKey("dbo.Acts", t => t.ActId)
                .Index(t => t.ActId)
                .Index(t => t.WorkTypeId)
                .Index(t => t.ActPartId)
                .Index(t => t.WorkBackAdapterId);
            
            CreateTable(
                "dbo.FullWorks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        WorkId = c.Int(),
                        Sum = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Works", t => t.WorkId)
                .Index(t => t.WorkId);
            
            CreateTable(
                "dbo.Regions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        WorkId = c.Int(),
                        Number = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Works", t => t.WorkId)
                .Index(t => t.WorkId);
            
            CreateTable(
                "dbo.WorkBackAdapters",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BackId = c.Int(),
                        Text = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Backs", t => t.BackId)
                .Index(t => t.BackId);
            
            CreateTable(
                "dbo.Backs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EpisodeId = c.Int(),
                        BackTypeId = c.Int(),
                        Name = c.String(),
                        BaseBackId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BackTypes", t => t.BackTypeId)
                .ForeignKey("dbo.Backs", t => t.BaseBackId)
                .ForeignKey("dbo.Episodes", t => t.EpisodeId)
                .Index(t => t.EpisodeId)
                .Index(t => t.BackTypeId)
                .Index(t => t.BaseBackId);
            
            CreateTable(
                "dbo.BackTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Episodes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProjectId = c.Int(),
                        Name = c.String(),
                        Number = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Projects", t => t.ProjectId)
                .Index(t => t.ProjectId);
            
            CreateTable(
                "dbo.Projects",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AlternativeProjectNames",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProjectId = c.Int(),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Projects", t => t.ProjectId)
                .Index(t => t.ProjectId);
            
            CreateTable(
                "dbo.Minigames",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BackId = c.Int(),
                        Number = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Backs", t => t.BackId)
                .Index(t => t.BackId);
            
            CreateTable(
                "dbo.CountRegions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BackId = c.Int(),
                        Count = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Backs", t => t.BackId)
                .Index(t => t.BackId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Works", "ActId", "dbo.Acts");
            DropForeignKey("dbo.Works", "WorkTypeId", "dbo.WorkTypes");
            DropForeignKey("dbo.Works", "WorkBackAdapterId", "dbo.WorkBackAdapters");
            DropForeignKey("dbo.WorkBackAdapters", "BackId", "dbo.Backs");
            DropForeignKey("dbo.CountRegions", "BackId", "dbo.Backs");
            DropForeignKey("dbo.Minigames", "BackId", "dbo.Backs");
            DropForeignKey("dbo.Episodes", "ProjectId", "dbo.Projects");
            DropForeignKey("dbo.AlternativeProjectNames", "ProjectId", "dbo.Projects");
            DropForeignKey("dbo.Backs", "EpisodeId", "dbo.Episodes");
            DropForeignKey("dbo.Backs", "BaseBackId", "dbo.Backs");
            DropForeignKey("dbo.Backs", "BackTypeId", "dbo.BackTypes");
            DropForeignKey("dbo.Regions", "WorkId", "dbo.Works");
            DropForeignKey("dbo.FullWorks", "WorkId", "dbo.Works");
            DropForeignKey("dbo.Works", "ActPartId", "dbo.ActParts");
            DropForeignKey("dbo.WorkTypes", "TemplateTypeId", "dbo.TemplateTypes");
            DropForeignKey("dbo.Acts", "TemplateTypeId", "dbo.TemplateTypes");
            DropForeignKey("dbo.Humen", "SupportContractId", "dbo.Contracts");
            DropForeignKey("dbo.Humen", "DevelopmentContractId", "dbo.Contracts");
            DropForeignKey("dbo.Humen", "BankId", "dbo.Banks");
            DropForeignKey("dbo.Humen", "AddressId", "dbo.Addresses");
            DropForeignKey("dbo.Addresses", "StreetTypeId", "dbo.StreetTypes");
            DropForeignKey("dbo.Addresses", "LocalityTypeId", "dbo.LocalityTypes");
            DropForeignKey("dbo.Acts", "HumanId", "dbo.Humen");
            DropForeignKey("dbo.FullActs", "ActId", "dbo.Acts");
            DropIndex("dbo.CountRegions", new[] { "BackId" });
            DropIndex("dbo.Minigames", new[] { "BackId" });
            DropIndex("dbo.AlternativeProjectNames", new[] { "ProjectId" });
            DropIndex("dbo.Episodes", new[] { "ProjectId" });
            DropIndex("dbo.Backs", new[] { "BaseBackId" });
            DropIndex("dbo.Backs", new[] { "BackTypeId" });
            DropIndex("dbo.Backs", new[] { "EpisodeId" });
            DropIndex("dbo.WorkBackAdapters", new[] { "BackId" });
            DropIndex("dbo.Regions", new[] { "WorkId" });
            DropIndex("dbo.FullWorks", new[] { "WorkId" });
            DropIndex("dbo.Works", new[] { "WorkBackAdapterId" });
            DropIndex("dbo.Works", new[] { "ActPartId" });
            DropIndex("dbo.Works", new[] { "WorkTypeId" });
            DropIndex("dbo.Works", new[] { "ActId" });
            DropIndex("dbo.WorkTypes", new[] { "TemplateTypeId" });
            DropIndex("dbo.Addresses", new[] { "StreetTypeId" });
            DropIndex("dbo.Addresses", new[] { "LocalityTypeId" });
            DropIndex("dbo.Humen", new[] { "AddressId" });
            DropIndex("dbo.Humen", new[] { "SupportContractId" });
            DropIndex("dbo.Humen", new[] { "DevelopmentContractId" });
            DropIndex("dbo.Humen", new[] { "BankId" });
            DropIndex("dbo.FullActs", new[] { "ActId" });
            DropIndex("dbo.Acts", new[] { "TemplateTypeId" });
            DropIndex("dbo.Acts", new[] { "HumanId" });
            DropTable("dbo.CountRegions");
            DropTable("dbo.Minigames");
            DropTable("dbo.AlternativeProjectNames");
            DropTable("dbo.Projects");
            DropTable("dbo.Episodes");
            DropTable("dbo.BackTypes");
            DropTable("dbo.Backs");
            DropTable("dbo.WorkBackAdapters");
            DropTable("dbo.Regions");
            DropTable("dbo.FullWorks");
            DropTable("dbo.Works");
            DropTable("dbo.WorkTypes");
            DropTable("dbo.TemplateTypes");
            DropTable("dbo.Contracts");
            DropTable("dbo.Banks");
            DropTable("dbo.StreetTypes");
            DropTable("dbo.LocalityTypes");
            DropTable("dbo.Addresses");
            DropTable("dbo.Humen");
            DropTable("dbo.FullActs");
            DropTable("dbo.Acts");
            DropTable("dbo.ActParts");
        }
    }
}

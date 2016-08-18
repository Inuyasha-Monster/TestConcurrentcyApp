namespace TestConcurrentcyApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddInputAndOutput : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Students", newName: "Student");
            CreateTable(
                "dbo.InputAccount",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 8),
                        Balance = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.OutputAccount",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 8),
                        Balance = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.OutputAccount");
            DropTable("dbo.InputAccount");
            RenameTable(name: "dbo.Student", newName: "Students");
        }
    }
}

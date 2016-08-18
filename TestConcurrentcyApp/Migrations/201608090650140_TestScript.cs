namespace TestConcurrentcyApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TestScript : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TestCreateScript",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TestCreateScript");
        }
    }
}

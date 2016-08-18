namespace TestConcurrentcyApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateLastName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TestCreateScript", "Name", c => c.String(nullable: false, maxLength: 18));
            AlterColumn("dbo.Student", "LastName", c => c.String(nullable: false, maxLength: 18));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Student", "LastName", c => c.String());
            DropColumn("dbo.TestCreateScript", "Name");
        }
    }
}

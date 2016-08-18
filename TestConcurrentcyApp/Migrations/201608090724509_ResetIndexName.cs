namespace TestConcurrentcyApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class ResetIndexName : DbMigration
    {
        public override void Up()
        {
            this.RenameIndex("Student", "Create_Name_Index", "RenameIndex");
        }

        public override void Down()
        {
            this.RenameIndex("Student", "RenameIndex", "Create_Name_Index");
        }
    }
}

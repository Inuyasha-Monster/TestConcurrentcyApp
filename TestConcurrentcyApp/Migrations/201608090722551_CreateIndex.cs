namespace TestConcurrentcyApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class CreateIndex : DbMigration
    {
        public override void Up()
        {
            this.CreateIndex("Student", "LastName", false, name: "Create_Name_Index");
        }

        public override void Down()
        {
            this.DropIndex("Student", "Create_Name_Index");
        }
    }
}

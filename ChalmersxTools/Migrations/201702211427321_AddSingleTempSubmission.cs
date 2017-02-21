namespace ChalmersxTools.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSingleTempSubmission : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SingleTemperatureMeasurementSubmissions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(maxLength: 64),
                        ContextId = c.String(maxLength: 128),
                        Position_Latitude = c.Double(nullable: false),
                        Position_Longitude = c.Double(nullable: false),
                        Time = c.DateTime(nullable: false),
                        Measurement = c.Double(),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => new { t.UserId, t.ContextId }, unique: true, name: "IX_UserInContext");
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.SingleTemperatureMeasurementSubmissions", "IX_UserInContext");
            DropTable("dbo.SingleTemperatureMeasurementSubmissions");
        }
    }
}

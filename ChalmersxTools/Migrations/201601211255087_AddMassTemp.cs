namespace ChalmersxTools.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMassTemp : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EarthMassSubmissions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(maxLength: 64),
                        CourseOrg = c.String(maxLength: 64),
                        CourseId = c.String(maxLength: 64),
                        CourseRun = c.String(maxLength: 64),
                        MeanGravityAcceleration = c.Double(nullable: false),
                        TotalEarthMass = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => new { t.UserId, t.CourseOrg, t.CourseId, t.CourseRun }, unique: true, name: "IX_UserInCourseRun");
            
            CreateTable(
                "dbo.TemperatureMeasurementSubmissions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(maxLength: 64),
                        CourseOrg = c.String(maxLength: 64),
                        CourseId = c.String(maxLength: 64),
                        CourseRun = c.String(maxLength: 64),
                        Position_Latitude = c.Double(nullable: false),
                        Position_Longitude = c.Double(nullable: false),
                        Measurement1 = c.Double(),
                        Measurement2 = c.Double(),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => new { t.UserId, t.CourseOrg, t.CourseId, t.CourseRun }, unique: true, name: "IX_UserInCourseRun");
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.TemperatureMeasurementSubmissions", "IX_UserInCourseRun");
            DropIndex("dbo.EarthMassSubmissions", "IX_UserInCourseRun");
            DropTable("dbo.TemperatureMeasurementSubmissions");
            DropTable("dbo.EarthMassSubmissions");
        }
    }
}

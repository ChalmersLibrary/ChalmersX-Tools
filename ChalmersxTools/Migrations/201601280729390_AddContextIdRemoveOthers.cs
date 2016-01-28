namespace ChalmersxTools.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddContextIdRemoveOthers : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.EarthMassSubmissions", "IX_UserInCourseRun");
            DropIndex("dbo.EarthSpheresImagesSubmissions", "IX_UserInCourseRun");
            DropIndex("dbo.LtiSessions", "IX_StudentWithToolOnCourseRun");
            DropIndex("dbo.StudentPresentations", "IX_UserInCourseRun");
            DropIndex("dbo.TemperatureMeasurementSubmissions", "IX_UserInCourseRun");
            AddColumn("dbo.EarthMassSubmissions", "ContextId", c => c.String(maxLength: 128));
            AddColumn("dbo.EarthSpheresImagesSubmissions", "ContextId", c => c.String(maxLength: 128));
            AddColumn("dbo.LtiSessions", "ContextId", c => c.String(maxLength: 128));
            AddColumn("dbo.StudentPresentations", "ContextId", c => c.String(maxLength: 128));
            AddColumn("dbo.TemperatureMeasurementSubmissions", "ContextId", c => c.String(maxLength: 128));
            CreateIndex("dbo.EarthMassSubmissions", new[] { "UserId", "ContextId" }, unique: true, name: "IX_UserInContext");
            CreateIndex("dbo.EarthSpheresImagesSubmissions", new[] { "UserId", "ContextId" }, unique: true, name: "IX_UserInContext");
            CreateIndex("dbo.LtiSessions", new[] { "ConsumerKey", "UserId", "ContextId" }, unique: true, name: "IX_StudentWithToolInContext");
            CreateIndex("dbo.StudentPresentations", new[] { "UserId", "ContextId" }, unique: true, name: "IX_UserInContext");
            CreateIndex("dbo.TemperatureMeasurementSubmissions", new[] { "UserId", "ContextId" }, unique: true, name: "IX_UserInContext");
            DropColumn("dbo.EarthMassSubmissions", "CourseOrg");
            DropColumn("dbo.EarthMassSubmissions", "CourseId");
            DropColumn("dbo.EarthMassSubmissions", "CourseRun");
            DropColumn("dbo.EarthSpheresImagesSubmissions", "CourseOrg");
            DropColumn("dbo.EarthSpheresImagesSubmissions", "CourseId");
            DropColumn("dbo.EarthSpheresImagesSubmissions", "CourseRun");
            DropColumn("dbo.LtiSessions", "CourseOrg");
            DropColumn("dbo.LtiSessions", "CourseId");
            DropColumn("dbo.LtiSessions", "CourseRun");
            DropColumn("dbo.StudentPresentations", "CourseOrg");
            DropColumn("dbo.StudentPresentations", "CourseId");
            DropColumn("dbo.StudentPresentations", "CourseRun");
            DropColumn("dbo.TemperatureMeasurementSubmissions", "CourseOrg");
            DropColumn("dbo.TemperatureMeasurementSubmissions", "CourseId");
            DropColumn("dbo.TemperatureMeasurementSubmissions", "CourseRun");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TemperatureMeasurementSubmissions", "CourseRun", c => c.String(maxLength: 64));
            AddColumn("dbo.TemperatureMeasurementSubmissions", "CourseId", c => c.String(maxLength: 64));
            AddColumn("dbo.TemperatureMeasurementSubmissions", "CourseOrg", c => c.String(maxLength: 64));
            AddColumn("dbo.StudentPresentations", "CourseRun", c => c.String(maxLength: 64));
            AddColumn("dbo.StudentPresentations", "CourseId", c => c.String(maxLength: 64));
            AddColumn("dbo.StudentPresentations", "CourseOrg", c => c.String(maxLength: 64));
            AddColumn("dbo.LtiSessions", "CourseRun", c => c.String(maxLength: 64));
            AddColumn("dbo.LtiSessions", "CourseId", c => c.String(maxLength: 64));
            AddColumn("dbo.LtiSessions", "CourseOrg", c => c.String(maxLength: 64));
            AddColumn("dbo.EarthSpheresImagesSubmissions", "CourseRun", c => c.String(maxLength: 64));
            AddColumn("dbo.EarthSpheresImagesSubmissions", "CourseId", c => c.String(maxLength: 64));
            AddColumn("dbo.EarthSpheresImagesSubmissions", "CourseOrg", c => c.String(maxLength: 64));
            AddColumn("dbo.EarthMassSubmissions", "CourseRun", c => c.String(maxLength: 64));
            AddColumn("dbo.EarthMassSubmissions", "CourseId", c => c.String(maxLength: 64));
            AddColumn("dbo.EarthMassSubmissions", "CourseOrg", c => c.String(maxLength: 64));
            DropIndex("dbo.TemperatureMeasurementSubmissions", "IX_UserInContext");
            DropIndex("dbo.StudentPresentations", "IX_UserInContext");
            DropIndex("dbo.LtiSessions", "IX_StudentWithToolInContext");
            DropIndex("dbo.EarthSpheresImagesSubmissions", "IX_UserInContext");
            DropIndex("dbo.EarthMassSubmissions", "IX_UserInContext");
            DropColumn("dbo.TemperatureMeasurementSubmissions", "ContextId");
            DropColumn("dbo.StudentPresentations", "ContextId");
            DropColumn("dbo.LtiSessions", "ContextId");
            DropColumn("dbo.EarthSpheresImagesSubmissions", "ContextId");
            DropColumn("dbo.EarthMassSubmissions", "ContextId");
            CreateIndex("dbo.TemperatureMeasurementSubmissions", new[] { "UserId", "CourseOrg", "CourseId", "CourseRun" }, unique: true, name: "IX_UserInCourseRun");
            CreateIndex("dbo.StudentPresentations", new[] { "UserId", "CourseOrg", "CourseId", "CourseRun" }, unique: true, name: "IX_UserInCourseRun");
            CreateIndex("dbo.LtiSessions", new[] { "ConsumerKey", "UserId", "CourseOrg", "CourseId", "CourseRun" }, unique: true, name: "IX_StudentWithToolOnCourseRun");
            CreateIndex("dbo.EarthSpheresImagesSubmissions", new[] { "UserId", "CourseOrg", "CourseId", "CourseRun" }, unique: true, name: "IX_UserInCourseRun");
            CreateIndex("dbo.EarthMassSubmissions", new[] { "UserId", "CourseOrg", "CourseId", "CourseRun" }, unique: true, name: "IX_UserInCourseRun");
        }
    }
}

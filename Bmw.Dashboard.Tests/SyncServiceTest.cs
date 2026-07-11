using Bmw.Dashboard.Core.Data.DbContexts;
using Bmw.Dashboard.Core.Interfaces;
using Bmw.Dashboard.Core.Models.API;
using Bmw.Dashboard.Core.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Bmw.Dashboard.Tests;

public class SyncServiceTest
{
    [Fact]
    public async Task SyncVehicleData_WhenNewDataAvailable_ShouldSaveToDb()
    {

    }
}

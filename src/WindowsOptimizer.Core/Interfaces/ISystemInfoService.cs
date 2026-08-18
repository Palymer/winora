using WindowsOptimizer.Core.Models;

namespace WindowsOptimizer.Core.Interfaces;

public interface ISystemInfoService
{
    SystemSnapshot GetSnapshot();
}

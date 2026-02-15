using TradeProof.Data;
using TradeProof.Training;

namespace TradeProof.Core
{
    public interface ITaskTracker
    {
        void Initialize(TaskDefinition definition, TaskMode mode);
        void Reset();
        float GetScore();
        float GetCompletionPercentage();
        TaskResult BuildResult();
    }
}

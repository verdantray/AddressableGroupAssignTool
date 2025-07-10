using System.Text;

namespace AddressableGroupAssignTool.Editor
{
    public class AssignLogBuilder
    {
        private const string SUCCESS_FORMAT = "{0} moves from '{1}' -> to '{2}'\n";
        private const string FAILED_FORMAT = "{0} failed to move... remains {1}\n";
        private const string REMAIN_FORMAT = "{0} need not to move group\n";
        
        private readonly StringBuilder _stringBuilder = new StringBuilder();

        public void AppendSuccessResult(string assetName, string parentGroupName, string targetGroupName)
        {
            AppendFormat(SUCCESS_FORMAT, assetName, parentGroupName, targetGroupName);
        }

        public void AppendFailedResult(string assetName, string parentGroupName)
        {
            AppendFormat(FAILED_FORMAT, assetName, parentGroupName);
        }

        public void AppendRemainResult(string assetName)
        {
            AppendFormat(REMAIN_FORMAT, assetName);
        }

        public void Append(string toAppend)
        {
            _stringBuilder.Append(toAppend);
        }

        public void AppendLine(string toAppend)
        {
            _stringBuilder.AppendLine(toAppend);
        }

        public void AppendFormat(string format, params object[] toAppends)
        {
            _stringBuilder.AppendFormat(format, toAppends);
        }

        public void Clear()
        {
            _stringBuilder.Clear();
        }

        public override string ToString()
        {
            return _stringBuilder.ToString();
        }
    }
}

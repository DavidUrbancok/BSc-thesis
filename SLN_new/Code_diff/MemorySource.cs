using System;

namespace CMS.DataEngine.Query.Sources
{
    /// <summary>
    /// Data query source which gets the data from the memory.
    /// </summary>
    public class MemorySource : QuerySource
    {
        private DataQuerySource LeftDataSource { get; }


        private DataQuerySource RightDataSource;


        /// <summary>
        /// Result of the join operation.
        /// </summary>
        public DataQuerySource mResultDataSource;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataSource">Data source</param>
        /// <param name="querySource">Query source</param>
        public MemorySource(DataQuerySource dataSource, QuerySource querySource)
            : base(querySource)
        {
            LeftDataSource = dataSource;
        }


        /// <summary>
        /// Performs a join on the given sources.
        /// </summary>
        /// <typeparam name="TObject">Type of the object source</typeparam>
        /// <param name="leftColumn">Left column</param>
        /// <param name="rightColumn">Right column</param>
        /// <param name="joinType">Join type</param>
        /// <param name="additionalCondition">Additional condition</param>
        public override QuerySource Join<TObject>(string leftColumn, string rightColumn, JoinTypeEnum joinType = JoinTypeEnum.Inner, IWhereCondition additionalCondition = null)
        {
            ObjectSource<TObject> objectSource;

            PrepareDataSources(out objectSource, leftColumn, rightColumn, additionalCondition, joinType);

            return Join(objectSource, leftColumn, rightColumn, additionalCondition, joinType);
        }

        
        /// <summary>
        /// Performs an inner join on the given sources.
        /// </summary>
        /// <typeparam name="TObject">Type of the object source</typeparam>
        /// <param name="leftColumn">Left column</param>
        /// <param name="rightColumn">Right column</param>
        /// <param name="additionalCondition">Additional condition</param>
        public override QuerySource InnerJoin<TObject>(string leftColumn, string rightColumn, IWhereCondition additionalCondition = null)
        {
            ObjectSource<TObject> objectSource;

            PrepareDataSources(out objectSource, leftColumn, rightColumn, additionalCondition);

            return Join(objectSource, leftColumn, rightColumn, additionalCondition);
        }

        
        /// <summary>
        /// Performs a left outer join on the given sources.
        /// </summary>
        /// <typeparam name="TObject">Type of the object source</typeparam>
        /// <param name="leftColumn">Left column</param>
        /// <param name="rightColumn">Right column</param>
        /// <param name="additionalCondition">Additional condition</param>
        public override QuerySource LeftJoin<TObject>(string leftColumn, string rightColumn, IWhereCondition additionalCondition = null)
        {
            ObjectSource<TObject> objectSource;
            
            PrepareDataSources(out objectSource, leftColumn, rightColumn, additionalCondition, JoinTypeEnum.LeftOuter);

            return LeftJoin(objectSource, leftColumn, rightColumn, additionalCondition);
        }

        
        /// <summary>
        /// Performs a right outer join on the given sources.
        /// </summary>
        /// <typeparam name="TObject">Type of the object source</typeparam>
        /// <param name="leftColumn">Left column</param>
        /// <param name="rightColumn">Right column</param>
        /// <param name="additionalCondition">Additional condition</param>
        public override QuerySource RightJoin<TObject>(string leftColumn, string rightColumn, IWhereCondition additionalCondition = null)
        {
            ObjectSource<TObject> objectSource;

            PrepareDataSources(out objectSource, leftColumn, rightColumn, additionalCondition, JoinTypeEnum.RightOuter);

            return RightJoin(objectSource, leftColumn, rightColumn, additionalCondition);
        }


        /// <summary>
        /// Prepares the right data source for the join operation.
        /// The type of the join operation is passed via <paramref name="joinType"/> parameter.
        /// </summary>
        /// <typeparam name="TObject">Type of the object source</typeparam>
        /// <param name="objectSource">Object source</param>
        /// <param name="leftColumn">Left column</param>
        /// <param name="rightColumn">Right column</param>
        /// <param name="additionalCondition">Additional condition</param>
        /// <param name="joinType">Join type</param>
        private void PrepareDataSources<TObject>(out ObjectSource<TObject> objectSource, string leftColumn, string rightColumn,
                                                 IWhereCondition additionalCondition = null, JoinTypeEnum joinType = JoinTypeEnum.Inner)
            where TObject : BaseInfo, new()
        {
            objectSource = new ObjectSource<TObject>();
            objectSource.SourceExpression = GetSourceExpression(new QueryDataParameters());

            var provider = InfoProviderLoader.GetInfoProvider(objectSource.ObjectType) as ITestableProvider;
            if (provider == null)
            {
                throw new InvalidOperationException($"Info provider for object type '{objectSource.ObjectType}' does not implement '{typeof(ITestableProvider).FullName}'." +
                    "The join operation failed.");
            }

            RightDataSource = provider.DataSource;

            // Join the two data sources
            mResultDataSource = InMemoryJoin.Join(LeftDataSource, RightDataSource, leftColumn, rightColumn, additionalCondition, joinType);
        }
    }
}
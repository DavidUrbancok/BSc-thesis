using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CMS.DataEngine.Query.Sources
{
    /// <summary>
    /// Provides functionality to join two DataSource objects in memory.
    /// </summary>
    public static class InMemoryJoin
    {
        /// <summary>
        /// Joins the given data sources in the memory according to the <paramref name="joinType"/>
        /// </summary>
        /// <param name="leftDataQuerySource">Left data query source</param>
        /// <param name="rightDataQuerySource">Right data query source</param>
        /// <param name="leftColumn">Left column name</param>
        /// <param name="rightColumn">Right column name</param>
        /// <param name="additionalCondition">Additional condition</param>
        /// <param name="joinType">Join type</param>
        public static DataQuerySource Join(DataQuerySource leftDataQuerySource, DataQuerySource rightDataQuerySource, string leftColumn, string rightColumn,
                                           IWhereCondition additionalCondition = null, JoinTypeEnum joinType = JoinTypeEnum.Inner)
        {
            var parameters = PrepareDataQuerySourceParameters();

            var leftTable = leftDataQuerySource.GetData(parameters).Tables[0].AsEnumerable();
            var rightTable = rightDataQuerySource.GetData(parameters).Tables[0].AsEnumerable();

            var result = JoinByJoinType(leftTable, rightTable, leftColumn, rightColumn, joinType);

            return ResultToMemoryDataQuery(result);
        }


        /// <summary>
        /// Performs a join operation based on <paramref name="joinType"/>.
        /// </summary>
        /// <param name="leftTable">Left table</param>
        /// <param name="rightTable">Right table</param>
        /// <param name="leftColumn">Left column</param>
        /// <param name="rightColumn">Right column</param>
        /// <param name="joinType">Join type</param>
        private static IEnumerable<DataRow> JoinByJoinType(IEnumerable<DataRow> leftTable, IEnumerable<DataRow> rightTable, string leftColumn, string rightColumn, JoinTypeEnum joinType)
        {
            switch (joinType)
            {
                case JoinTypeEnum.Inner:
                {
                    return InnerJoin(leftTable, rightTable, leftColumn, rightColumn);
                }
                case JoinTypeEnum.LeftOuter:
                {
                        return LeftOuterJoin(leftTable, rightTable, leftColumn, rightColumn);
                }
                case JoinTypeEnum.RightOuter:
                {
                    return RightOuterJoin(leftTable, rightTable, leftColumn, rightColumn);
                }
                default:
                {
                    return null;
                }
            }
        }


        /// <summary>
        /// Prepares the parameters for the data query source.
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="settings">Settings</param>
        /// <param name="offset">Offset</param>
        /// <param name="maxRecords">Maximum number of records</param>
        private static DataQuerySourceParameters PrepareDataQuerySourceParameters(ObjectQuery<DataClassInfo> query = null, DataQuerySettings settings = null, int offset = 0, int maxRecords = 0)
        {
            if (query == null)
            {
                query = DataClassInfoProvider.GetClasses();
            }

            if (settings == null)
            {
                settings = new DataQuerySettings();
            }

            return new DataQuerySourceParameters(query, settings, offset, maxRecords);
        }


        /// <summary>
        /// Performs an inner join on two DataTables.
        /// </summary>
        /// <param name="leftTable">Left table</param>
        /// <param name="rightTable">Right table</param>
        /// <param name="leftColumn">Left column</param>
        /// <param name="rightColumn">Right column</param>
        private static IEnumerable<DataRow> InnerJoin(IEnumerable<DataRow> leftTable, IEnumerable<DataRow> rightTable, string leftColumn, string rightColumn)
        {
            return leftTable.Join(
                rightTable,
                leftRow => leftRow.Field<int?>(leftColumn),
                rightRow => rightRow.Field<int?>(rightColumn),
                CombineDataRows);
        }


        /// <summary>
        /// Performs a left outer join on two DataTables.
        /// </summary>
        /// <param name="leftTable">Left table</param>
        /// <param name="rightTable">Right table</param>
        /// <param name="leftColumn">Left column</param>
        /// <param name="rightColumn">Right column</param>
        private static IEnumerable<DataRow> LeftOuterJoin(IEnumerable<DataRow> leftTable, IEnumerable<DataRow> rightTable, string leftColumn, string rightColumn)
        {
            return leftTable.GroupJoin(
                                rightTable,
                                leftRow => leftRow.Field<int?>(leftColumn),
                                rightRow => rightRow.Field<int?>(rightColumn),
                                (leftRow, matchingRightRows) => new
                                {
                                    leftRow,
                                    matchingRightRows
                                })
                            .SelectMany(
                                matchinPairOfRows => matchinPairOfRows.matchingRightRows.DefaultIfEmpty(),
                                (leftRowWithMatchingRightRows, rightRow) => CombineDataRows(leftRowWithMatchingRightRows.leftRow, rightRow));
        }


        /// <summary>
        /// Performs a right outer join on two DataTables.
        /// </summary>
        /// <param name="leftTable">Left table</param>
        /// <param name="rightTable">Right table</param>
        /// <param name="leftColumn">Left column</param>
        /// <param name="rightColumn">Right column</param>
        private static IEnumerable<DataRow> RightOuterJoin(IEnumerable<DataRow> leftTable, IEnumerable<DataRow> rightTable, string leftColumn, string rightColumn)
        {
            return LeftOuterJoin(rightTable, leftTable, rightColumn, leftColumn);
        }


        /// <summary>
        /// Combines two DataRow objects into a single DataRow.
        /// </summary>
        /// <param name="leftRow">Left row</param>
        /// <param name="rightRow">Right row</param>
        /// <returns></returns>
        private static DataRow CombineDataRows(DataRow leftRow, DataRow rightRow)
        {
            if (rightRow == null)
            {
                return leftRow;
            }

            var fields = leftRow.ItemArray.Concat(rightRow.ItemArray).ToArray();

            var targetTable = CreateTargetTable(leftRow.Table, rightRow.Table);

            targetTable.Rows.Add(fields);

            return targetTable.Rows[0];
        }


        /// <summary>
        /// Creates a table that is a concatenation of columns from <paramref name="leftTable"/> and <paramref name="rightTable"/>.
        /// </summary>
        /// <param name="leftTable">Left table</param>
        /// <param name="rightTable">Right table</param>
        private static DataTable CreateTargetTable(DataTable leftTable, DataTable rightTable)
        {
            var leftTableColumns = leftTable.Columns.OfType<DataColumn>()
                                            .Select(dc => new DataColumn(dc.ColumnName, dc.DataType, dc.Expression, dc.ColumnMapping));
            var rightTableColumns = rightTable.Columns.OfType<DataColumn>()
                                              .Select(dc => new DataColumn(dc.ColumnName, dc.DataType, dc.Expression, dc.ColumnMapping));

            var targetTable = new DataTable();
            targetTable.Columns.AddRange(leftTableColumns.ToArray());

            foreach (DataColumn column in rightTableColumns)
            {
                var newColumn = column;

                if (targetTable.Columns.Contains(newColumn.ColumnName))
                {
                    newColumn.ColumnName = rightTable.TableName + "_" + column.ColumnName;
                }

                targetTable.Columns.Add(newColumn);
            }

            return targetTable;
        }


        /// <summary>
        /// Transforms a query of data rows into a MemoryDataQuerySource.
        /// </summary>
        /// <param name="dataRows">Data table</param>
        private static DataQuerySource ResultToMemoryDataQuery(IEnumerable<DataRow> dataRows)
        {
            DataTable dataTable;

            try
            {
                dataTable = dataRows?.CopyToDataTable();
            }
            catch (InvalidOperationException)
            {
                // The result contains no data rows
                return null;
            }

            var dataSet = new DataSet();

            if (dataRows != null)
            {
                dataSet.Tables.Add(dataTable);
            }

            return new MemoryDataQuerySource(dataSet);
        }
    }
}


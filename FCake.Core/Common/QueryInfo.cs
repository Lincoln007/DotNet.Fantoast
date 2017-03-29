using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
#if !NET_2_0
using System.Runtime.Serialization;
#endif

namespace FCake.Framework
{
    public delegate QueryInfo FindByQueryInfo(QueryInfo info);
#if !NET_2_0
    [DataContract]
#endif
    public class QueryInfo : ICloneable
    {
        /// <summary>
        /// NamedQuery/SP/SQLʱ:QueryObject/CustomSQL
        /// </summary>
        public QueryInfo()
        {
        }
        /// <summary>
        /// queryObject: ��HQLʱ,�ɰ�������
        /// </summary>
        public QueryInfo(string queryObj)
        {
            this.QueryObject = queryObj;
        }

        private int startRecord;
#if !NET_2_0
        [DataMember]
#endif
        public int StartRecord
        {
            get { return startRecord; }
            set { startRecord = value; }
        }
        private int pageSize;
#if !NET_2_0
        [DataMember]
#endif
        public int PageSize
        {
            get
            {
                if (pageSize == 0) pageSize = 15;
                return pageSize;
            }
            set { pageSize = value; }
        }
        private int totalCount;
        /// <summary>
        /// ����Ϊ1 ʱ����ѯ��������ҳ�� ����Ϊ2ʱ������Ҫ���ҳ������ѯ����
        /// </summary>
#if !NET_2_0
        [DataMember]
#endif
        public int TotalCount
        {
            get { return totalCount; }
            set { totalCount = value; }
        }

        private IDictionary<string, string> where;
#if !NET_2_0
        [DataMember]
#endif
        public IDictionary<string, string> Where
        {
            get
            {
                if (where == null) where = new Dictionary<string, string>();
                return where;
            }
            set { where = value; }//deserialize
        }

        private IDictionary<string, string> filters;
        /// <summary>
        /// Add(Filter����������,Filter��)
        /// </summary>
#if !NET_2_0
        [DataMember]
#endif
        public IDictionary<string, string> Filters
        {
            get
            {
                if (filters == null) filters = new Dictionary<string, string>();
                return filters;
            }
            set { filters = value; }//deserialize
        }

        private List<string> orderBy;
#if !NET_2_0
        [DataMember]
#endif
        public List<string> OrderBy
        {
            get
            {
                if (orderBy == null) orderBy = new List<string>();
                return orderBy;
            }
            set { orderBy = value; }//deserialize
        }

        private string queryObject;
        /// <summary>
        /// ʵ�����ͱ���,���ŷָ����ʵ�塣 ��: Customer cust,...
        /// </summary>
#if !NET_2_0
        [DataMember]
#endif
        public string QueryObject
        {
            get { return queryObject; }
            set
            {
                queryObject = value;
                if (queryObject != null && queryObject.IndexOf(',') < 0)//only one entity
                {
                    int iAlias = queryObject.LastIndexOf(' ');
                    if (iAlias > 2)
                        this.alias = queryObject.Substring(iAlias + 1).Trim();//����
                }
            }
        }

        /// <summary>
        /// ��ѯ���� QueryObject=User u, �����Ϊ" u. "
        /// </summary>
#if !NET_2_0
        [DataMember]
#endif
        private string alias = string.Empty;

        /// <summary>
        /// ����Ϊ�����������ԣ���Find����ʱ���ݴ�����(alias.Property)���ˣ���i.Id , ��������CreatedOffice,CreatedBy
        /// </summary>
#if !NET_2_0
        [DataMember]
#endif
        public string AclProperty
        {
            get;
            set;
        }

        private string countField;
        /// <summary>
        /// Ĭ�Ͻ�����count(*)ͳ��
        /// </summary>
#if !NET_2_0
        [DataMember]
#endif
        public string CountField
        {
            get
            {
                if (countField == null) countField = "*";
                return countField;
            }
            set { countField = value; }
        }

        private string customSQL;
        /// <summary>
        /// �Զ���SQL/HQL���,��where����
        /// </summary>
#if !NET_2_0
        [DataMember]
#endif
        public string CustomSQL
        {
            get { return customSQL; }
            set { customSQL = value; }
        }
#if !NET_2_0
        [DataMember]
#endif
        //Դ��¼��
        public Int32 SoureceTotalCount
        {
            get;
            set;
        }
        /// <summary>
        /// ��SQL����Ϊfield +from +where +groupby +orderby
        /// </summary>
#if !NET_2_0
        [DataMember]
#endif
        public string GroupBy
        {
            get;
            set;
        }


        private string namedQuery;
        /// <summary>
        /// NamedQuery/SP
        /// ע�⣺Parameters�Ĳ���˳���������洢�����ڲ�˳����ͬ!
        /// </summary>
#if !NET_2_0
        [DataMember]
#endif
        public string NamedQuery
        {
            get { return namedQuery; }
            set { namedQuery = value; }
        }

        /// <summary>
        /// HQL without OrderBy string
        /// </summary>
        public string ToHQLString()
        {
            //if (!string.IsNullOrEmpty(this.AclProperty))
            //FireAclInjection();

            StringBuilder sb = new StringBuilder();
            if (this.CustomSQL == null)//no statement customized,let's build it.
            {
                if (this.QueryObject != null && this.QueryObject.IndexOf(" from ", StringComparison.InvariantCultureIgnoreCase) < 0)
                    sb.Append(" from ");
                sb.Append(this.QueryObject);
                if (this.Where.Count > 0)
                    sb.Append(" 1=1");
            }
            else
                BuildCustomSQL(sb);

            BuildWhere(sb);
            return sb.ToString();
        }

        //private void FireAclInjection()
        //{
        //    AclInjector.Invoke(_aclInjectorObj, new object[] { this });
        //}

        //static object _aclInjectorObj = null;
        //static System.Reflection.MethodInfo _aclInjector=null;
        //static System.Reflection.MethodInfo AclInjector 
        //{
        //    get
        //    {
        //        if (_aclInjector == null)
        //        {
        //            object injector = Framework.Proxy.Context.ApplicationContext.GetContext().GetObject("AclInjector");
        //            if (injector != null)
        //            {
        //                Spring.Objects.Support.MethodInvoker mi = new Spring.Objects.Support.MethodInvoker();
        //                mi.TargetObject = injector;
        //                mi.TargetMethod = "Invoke";
        //                mi.Arguments = new object[] { new QueryInfo() };
        //                mi.Prepare();

        //                _aclInjectorObj = injector;
        //               _aclInjector = mi.GetPreparedMethod();
        //            }
        //            else
        //                throw new Exception("����AclProertyע��ʱ��δ���ô���'Invoke(QueryInfo info)'�����Ķ���'AclInjector'.");
        //        }
        //        return _aclInjector;
        //    }
        //}

        /// <summary>
        /// SQL without OrderBy string
        /// </summary>
        public string ToSQLString()
        {
            StringBuilder sb = new StringBuilder();
            if (this.CustomSQL == null)//no statement customized,let's build it.
            {
                if (this.QueryObject != null && this.QueryObject.IndexOf(" from ", StringComparison.InvariantCultureIgnoreCase) < 0)
                    sb.Append(" from ");
                sb.Append(this.QueryObject);
                if (this.Where.Count > 0)
                    sb.Append(" 1=1");
            }
            else
                BuildCustomSQL(sb);
            //if (this.CustomSQL.IndexOf("select ") < 0)//not whole statement customized,let's build it.
            //    sb.Append("select * from ");            

            BuildWhere(sb);
            return sb.ToString();
        }
        public object[] GetParams()
        {
            object[] arr = new object[this.Parameters.Count];
            int i = -1;
            foreach (var item in this.Parameters)
            {
                i += 1;
                arr[i] = item.Value;
            }
            return arr;
        }
        private void BuildCustomSQL(StringBuilder sb)
        {
            //edit daidz 2014-02-28
            //int i = this.CustomSQL.IndexOf("order by");
            //if (i > 0)//��Order By��SQL��
            //{
            //    if (this.OrderBy.Count == 0)
            //        this.OrderBy.Add(this.CustomSQL.Substring(i + 8));
            //    this.CustomSQL = this.CustomSQL.Substring(0, i);
            //}
            sb.Append(this.CustomSQL);

            if (this.Where.Count > 0 && this.CustomSQL.IndexOf(" where ") < 0)//׼����where�����
                sb.Append(" 1=1");
        }
        private void BuildWhere(StringBuilder sb)
        {
            //where fragment
            if (this.Where.Count > 0)
            {
                foreach (string con in this.Where.Values)
                    sb.Append(" ").Append(con);
            }
            if (!string.IsNullOrEmpty(GroupBy))
            {
                sb.Append(" group by ");
                sb.Append(GroupBy);
            }
        }

        /// <summary>
        /// OrderBy is not supported when 'COUNT' query. so seperate it.
        /// </summary>
        public string ToOrderBy()
        {
            StringBuilder sb = new StringBuilder();
            //order by fragment
            if (this.OrderBy.Count > 0)
            {
                sb.Append(" order by ");
                foreach (string ord in this.OrderBy)
                    sb.Append(ord).Append(",");
                sb.Remove(sb.Length - 1, 1);//last comm
            }
            return sb.ToString();
        }

        private static readonly string EQ_EXPRESSION = "and {0} = @{1}";//Ĭ��=���ʽ

        /// <summary>
        /// �ؼ���Ϊ prm_Name_LLK_u_ �ֱ�Ϊprm_{�ֶ���}_{ƥ������?}_{����?}_. ƥ�估�������п���
        /// �磺prm_Name_u_ , prm_Name_GT_ , prm_Time_u_V2 ����Ч
        /// </summary>
        public void AddParam(object dictionary)
        {
            AddParam((IDictionary)dictionary);
        }

        static readonly Regex dtRegex = new Regex(@"^\d{2,4}-\d{1,2}-\d{1,2}", RegexOptions.Compiled);
        static readonly Regex paramRegex = new Regex(@"^(?<NAME>\w*?){1}(_(?<PAT>\w{2,3}))?(_(?<ALIAS>\w))?$", RegexOptions.Compiled);
        private object ChangeDbType(object valOld)
        {
            string val = valOld as string;
            if (val != null)
            {
                if (dtRegex.Match(val).Success)
                    return DateTime.Parse(val);
                int n;
                if (int.TryParse(val.ToString(), out n))
                    return n;
            }
            return valOld;
        }
        private void AddParam(IDictionary ps)
        {
            var j = -1;
            foreach (string key in ps.Keys)
            {
                j += 1;
                if (ps[key] != null)
                {
                    object val = ps[key];//����ֵ
                    string type = "";
                    //val = val.Trim();
                    if (val != null && !string.IsNullOrEmpty(val.ToString()))
                    {
                        Match m = paramRegex.Match(key);//{Field}_{LLK}_{u}
                        string sField = m.Groups["NAME"].Value;
                        string alias = m.Groups["ALIAS"].Value;
                        string patten = m.Groups["PAT"].Value;
                        if (!string.IsNullOrEmpty(patten))//'LLK,GEQ'
                        {
                            bool bChangedDbType = false;//����������Ҫת������
                            string sParam = key;//������
                            StringBuilder exp = new StringBuilder("and ");//where query expression

                            switch (patten)
                            {
                                #region Like����
                                case "LLK"://Left like? like '%value'
                                    val = val.ToString();
                                    type = "EndsWith";
                                    break;
                                case "RLK"://Right like? like 'value%'
                                    val = val.ToString();
                                    type = "StartsWith";
                                    break;
                                case "LK"://Left like? like '%value%'
                                    val = val.ToString();
                                    type = "Contains";
                                    break;
                                #endregion

                                #region ���ڣ�С��
                                case "GT":
                                    exp.AppendFormat("{0} > @{1}", sField, j);
                                    bChangedDbType = true;
                                    break;
                                case "GEQ":
                                    exp.AppendFormat("{0} >= @{1}", sField, j);
                                    bChangedDbType = true;
                                    break;
                                case "LT":
                                    exp.AppendFormat("{0} < @{1}", sField, j);
                                    bChangedDbType = true;
                                    break;
                                case "LEQ":
                                    exp.AppendFormat("{0} <= @{1}", sField, j);
                                    bChangedDbType = true;
                                    break;
                                case "NEQ":
                                    exp.AppendFormat("{0} <> @{1}", sField, j);
                                    bChangedDbType = true;
                                    break;
                                case "EQ":
                                    exp.AppendFormat("{0} = @{1}", sField, j);
                                    bChangedDbType = true;
                                    break;
                                #endregion

                                case "NVL":
                                    exp.AppendFormat("{0} is null", sField);
                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException("Match 'patten' is not valid.");
                            }
                            #region Like ���ֶ���װ
                            if (patten.EndsWith("LK"))
                            {
                                exp.Append("(");
                                //string[] fields = sField.Split('_');
                                //sParam = fields[0] + fields.Length.ToString();//�������ò�����
                                //for (int i = 0; i < fields.Length; i++)//����ÿ���ֶ�
                                //{
                                //    exp.AppendFormat("{{1}}{0}.Contains(@) or ", fields[i], sParam);//fieldName like :ParamName
                                //}
                                exp.AppendFormat("{0}.{1}(@{2}) or ", sField,type, j);//fieldName like :ParamName
                                exp.Remove(exp.Length - 4, 4);
                                exp.Append(")");
                            }
                            #endregion
                            AddParam(alias, sParam, (bChangedDbType && val is string) ? ChangeDbType(val) : val, exp.ToString());
                        }
                        else {
                            string exp = string.Format(EQ_EXPRESSION, sField, j);
                            AddParam(alias, key, ChangeDbType(val), exp);
                        }
                    }
                }//val not null
                else
                    AddParam(null, key, null, string.Empty);

            }//foreach
        }
        /// <summary>
        /// Ϊ����ṩ����ֵ,����NamedQuery,����CustomSQL��where�о������ڴ˲���,���Զ���� and {0}=:{0}
        /// </summary>
        public void AddParam(string namedParam, object value)
        {
            AddParam(namedParam, value, EQ_EXPRESSION);
        }
        /// <summary>
        /// Ϊ����ṩ����ֵ,��value=null,value=>DBNull.Value
        /// </summary>
        public void AddParam(string namedParam, object value, string expression)
        {
            string pAlias = this.alias;
            int i = namedParam.IndexOf(".");
            if (i > 0)//�����������
            {
                string[] p = namedParam.Split('.');
                pAlias = p[0];//ʵ�����
                namedParam = p[1];//������
            }
            AddParam(pAlias, namedParam, value, expression);
        }

        private void AddParam(string alias, string namedParam, object value, string expression)
        {
            if (this.NamedQuery != null)//sp?
            {
                if (Parameters.ContainsKey(namedParam))
                    throw new ArgumentException("��ͼ�ظ����ͬһ������:" + namedParam);
                else
                    Parameters.Add(namedParam, value);

                return;
            }

            if (value != null)
            {
                if (Parameters.ContainsKey(namedParam))
                    throw new ArgumentException("��ͼ�ظ����ͬһ������:" + namedParam);
                else
                    Parameters.Add(namedParam, value);
            }
            else if (expression.IndexOf(":" + namedParam) > 0 || expression.IndexOf(":{0}") > 0)//��Param������Sql��,������ӵ�Parameters���Ƴ�expression
                expression = "and {1}{0} is null";

            if (!string.IsNullOrEmpty(alias))
                alias += ".";
            if ((this.CustomSQL == null || this.CustomSQL.IndexOf(":" + namedParam) < 0) //not in sql text
                && !Where.ContainsKey(namedParam)) //not in where
                this.Where.Add(namedParam, string.Format(expression, namedParam, alias));//add to where
        }
        private IDictionary<string, object> parameters;
        /// <summary>
        /// ���롢���������
        /// </summary>
#if !NET_2_0
        [DataMember]
#endif
        public IDictionary<string, object> Parameters
        {
            get
            {
                if (parameters == null) parameters = new Dictionary<string, object>();
                return parameters;
            }
            set
            {
                parameters = value;
            }
        }


        private string[] initProps;
        /// <summary>
        /// Properties should be initialized
        /// </summary>
#if !NET_2_0
        [DataMember]
#endif
        public string[] InitProps
        {
            get { return initProps; }
            set { initProps = value; }
        }
        private string[] unInitProps;
        /// <summary>
        /// Properties should not be init!
        /// </summary>
#if !NET_2_0
        [DataMember]
#endif
        public string[] UnInitProps
        {
            get { return unInitProps; }
            set { unInitProps = value; }
        }

        private IList list;
        /// <summary>
        /// Query result to be returned!
        /// </summary>
#if !NET_2_0
        [DataMember]
#endif
        public IList List
        {
            get { return list; }
            set { list = value; }
        }

        /// <summary>
        /// Resultset should be transform to which type.
        /// It can be typeof(Entity) or Entity.FullName.
        /// </summary>
#if !NET_2_0
        [DataMember]
#endif
        public object Transformer
        {
            get;
            set;
        }

        /// <summary>
        /// �ظ�ʹ��ʵ��ʱ,�����³�ʼ��
        /// </summary>
        public void Init()
        {
            this.QueryObject = null;
            this.StartRecord = 0;
            this.PageSize = 0;
            this.TotalCount = 0;
            this.alias = string.Empty;
            this.Where.Clear();
            this.Filters.Clear();
            this.OrderBy.Clear();
            this.CountField = null;
            this.CustomSQL = null;
            this.NamedQuery = null;
            this.Parameters.Clear();
            this.InitProps = null;
            this.UnInitProps = null;
            this.List = null;
            this.Transformer = null;
        }

        public object Clone()
        {
            QueryInfo info = new QueryInfo();
            IEnumerator<KeyValuePair<string, object>> enumer = this.Parameters.GetEnumerator();
            while (enumer.MoveNext())
                info.Parameters.Add(enumer.Current.Key, enumer.Current.Value);

            IEnumerator<KeyValuePair<string, string>> senumer = this.Where.GetEnumerator();
            while (senumer.MoveNext())
                info.Where.Add(senumer.Current.Key, senumer.Current.Value);

            IEnumerator<KeyValuePair<string, string>> fenumer = this.Filters.GetEnumerator();
            while (fenumer.MoveNext())
                info.Filters.Add(fenumer.Current.Key, fenumer.Current.Value);

            info.CountField = this.CountField;
            info.CustomSQL = this.CustomSQL;
            info.InitProps = this.InitProps;
            info.NamedQuery = this.NamedQuery;
            info.OrderBy = this.OrderBy;
            info.PageSize = this.PageSize;
            info.QueryObject = this.QueryObject;
            info.StartRecord = this.StartRecord;
            info.TotalCount = this.TotalCount;
            info.Transformer = this.Transformer;
            info.UnInitProps = this.UnInitProps;
            return info;
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new StringBuilder();
            sb.Append("\tTotalCount:");
            sb.Append(this.TotalCount);
            sb.Append(Environment.NewLine);

            sb.Append(this.NamedQuery ?? (this.ToSQLString() + this.ToOrderBy()));
            IEnumerator<KeyValuePair<string, object>> enumer = this.Parameters.GetEnumerator();
            while (enumer.MoveNext())
            {
                sb.Append(Environment.NewLine);
                sb.Append("\t");
                sb.Append(enumer.Current.Key);
                sb.Append(":");
                sb.Append(enumer.Current.Value);
            }

            return sb.ToString();
        }
        /// <summary>
        /// ϵ�л�����
        /// </summary>
        [DataMember]
        public string JsonReulst
        {
            get;
            set;
        }
        /// <summary>
        /// �Ƿ���Ҫϵ�л�
        /// </summary>
        [DataMember]
        public bool IsSerialize
        {
            get;
            set;
        }
    }
}

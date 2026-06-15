using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Diagnostics;

namespace HtmlCF.Controls
{
	public class BindTreeView : TreeView
	{
        #region Properties

        private bool m_autoBuild = true;
        public bool AutoBuildTree
        {
            get { return this.m_autoBuild; }
            set { this.m_autoBuild = value; }
        }

        private ContextMenuStrip m_nodecontextmenustrip = null;
        public ContextMenuStrip NodeContextMenuStrip
        {
            get { return m_nodecontextmenustrip; }
            set { m_nodecontextmenustrip = value; }
        }

        private object m_selecteditem;
        public object SelectedItem
        {
            get { return m_selecteditem; }
        }

        #endregion

        #region Constructor

        public BindTreeView()
		{
			InitializeComponent();
        }

        #endregion

        #region Destructor

        protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if( components != null )
					components.Dispose();
			}

			base.Dispose( disposing );
        }

        #endregion

        #region Component Designer generated code

        private Container components = null;

        [DebuggerStepThrough()]
		private void InitializeComponent()
		{
			components = new Container ();
		}

		#endregion

		#region Data Binding

		private CurrencyManager m_currencyManager = null;
		private String m_ValueMember;
		private String m_DisplayMember;
		private object m_oDataSource;

		[Category("Data")]
		public object DataSource
		{
			get { return m_oDataSource; }
			set
			{
				if ( value == null )
				{
					this.m_currencyManager = null;
					this.Nodes.Clear();
				}
				else
				{
					if ( !(value is IList || m_oDataSource is IListSource ) )
						throw (new System.Exception ("Invalid DataSource"));
					else
					{
						if ( value is IListSource )
						{
							IListSource myListSource = (IListSource) value;
							if ( myListSource.ContainsListCollection == true )
								throw (new System.Exception ("Invalid DataSource"));
						}
                        else if (value is IBindingList)
                        {
                            var bl = (IBindingList)value;
                            bl.ListChanged += new ListChangedEventHandler(bl_ListChanged);
                        }
						this.m_oDataSource = value;
						this.m_currencyManager = (CurrencyManager) this.BindingContext[value];
						if ( this.AutoBuildTree )
							BuildTree();
					}
				}
			}
		}

        private void bl_ListChanged(object sender, ListChangedEventArgs e)
        {
            switch (e.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                case ListChangedType.ItemDeleted:
                case ListChangedType.ItemMoved:
                    if (AutoBuildTree)
                        BuildTree();
                    break;
                case ListChangedType.ItemChanged:
                    UpdateItem(e.NewIndex);
                    break;
                case ListChangedType.PropertyDescriptorAdded:
                    break;
                case ListChangedType.PropertyDescriptorChanged:
                    break;
                case ListChangedType.PropertyDescriptorDeleted:
                    break;
                case ListChangedType.Reset:
                    break;
                default:
                    break;
            }
        }

        private void UpdateItem(int index)
        {
            IList innerList = this.m_currencyManager.List;

            if (index >= 0 && index < innerList.Count)
            {
                var n = (BindTreeNode)FindNodeByPosition(index);

                var pdDisplayloc = this.m_currencyManager.GetItemProperties()[this.DisplayMember];
                var pdValueloc = this.m_currencyManager.GetItemProperties()[this.ValueMember];

                var display = pdDisplayloc.GetValue(n.Item).ToString();
                var value = pdValueloc.GetValue(n.Item);

                n.Text = display;
                n.Value = value;
            }
        }


		[Category("Data")]
		public string ValueMember
		{
			get { return this.m_ValueMember; }
			set { this.m_ValueMember = value; }
		}

		[Category("Data")]
		public string DisplayMember
		{
			get { return this.m_DisplayMember; }
			set { this.m_DisplayMember = value; }
		}

		public object GetValue(int index)
		{
			IList innerList = this.m_currencyManager.List;
			if ( innerList != null )
			{
				if ( (this.ValueMember != "") && (index >= 0 && 0 < innerList.Count))
				{
					PropertyDescriptor pdValueMember;
					pdValueMember = this.m_currencyManager.GetItemProperties()[this.ValueMember];
					return pdValueMember.GetValue(innerList[index]);
				}
			}
			return null;
		}

		public object GetDisplay(int index)
		{
			IList innerList = this.m_currencyManager.List;
			if ( innerList != null )
			{
				if ( (this.DisplayMember != "") && (index >= 0 && 0 < innerList.Count))
				{
					PropertyDescriptor pdDisplayMember;
					pdDisplayMember= this.m_currencyManager.GetItemProperties()[this.ValueMember];
					return pdDisplayMember.GetValue (innerList[index]);
				}
			}
			return null;
		}

		#endregion

		#region Building the Tree

		private ArrayList treeGroups = new ArrayList();

		public void BuildTree()
		{
			this.Nodes.Clear();
			if ( (this.m_currencyManager != null) && (this.m_currencyManager.List != null) )
			{
				IList innerList = this.m_currencyManager.List;
				TreeNodeCollection currNode = this.Nodes;
				int currGroupIndex = 0; 
				int currListIndex = 0; 


				if ( this.treeGroups.Count > currGroupIndex )
				{
					Group currGroup = (Group) treeGroups[currGroupIndex];
					BindTreeNode myFirstNode = null;
					PropertyDescriptor pdGroupBy;
					PropertyDescriptor pdValue;
					PropertyDescriptor pdDisplay;

					pdGroupBy = this.m_currencyManager.GetItemProperties ()[currGroup.GroupBy];
					pdValue = this.m_currencyManager.GetItemProperties()[currGroup.ValueMember];
					pdDisplay = this.m_currencyManager.GetItemProperties()[currGroup.DisplayMember];

					string currGroupBy = null;
					if ( innerList.Count > currListIndex )
					{
						object currObject;
						while (currListIndex < innerList.Count)
						{
							currObject = innerList[currListIndex];
							if ( pdGroupBy.GetValue(currObject).ToString() != currGroupBy )
							{
								currGroupBy = pdGroupBy.GetValue(currObject).ToString();

								myFirstNode = new BindTreeNode (currGroup.Name, 
									pdDisplay.GetValue (currObject).ToString(),
									currObject,
                                    pdValue.GetValue(innerList[currListIndex]),
									currGroup.ImageIndex,
									currGroup.SelectedImageIndex,
									currListIndex);

                                if (m_nodecontextmenustrip != null)
                                    myFirstNode.ContextMenuStrip = m_nodecontextmenustrip;

								currNode.Add ((TreeNode) myFirstNode);
							}
							else
								AddNodes (currGroupIndex, ref currListIndex, myFirstNode.Nodes, currGroup.GroupBy);
						} // end while
					} // end if
				} // end if
				else
				{
					while (currListIndex < innerList.Count )
					{
						AddNodes (currGroupIndex, ref currListIndex, this.Nodes, "");
					}
				} // end else
				
				if ( this.Nodes.Count > 0 )
					this.SelectedNode = this.Nodes[0];

			} // end if
		}

		private void AddNodes(int currGroupIndex, ref int currentListIndex, TreeNodeCollection currNodes, String prevGroupByField)
		{
			IList innerList = this.m_currencyManager.List;
			System.ComponentModel.PropertyDescriptor pdPrevGroupBy = null; 
			string prevGroupByValue = null;;
			Group currGroup;

			if ( prevGroupByField != "" )
				pdPrevGroupBy = this.m_currencyManager.GetItemProperties()[prevGroupByField];

			currGroupIndex += 1;

			if ( treeGroups.Count > currGroupIndex )
			{
				currGroup = ( Group) treeGroups[currGroupIndex];
				PropertyDescriptor pdGroupBy = null;
				PropertyDescriptor pdValue = null;
				PropertyDescriptor pdDisplay = null;

				pdGroupBy = this.m_currencyManager.GetItemProperties()[currGroup.GroupBy];
				pdValue = this.m_currencyManager.GetItemProperties()[currGroup.ValueMember];
				pdDisplay = this.m_currencyManager.GetItemProperties()[currGroup.DisplayMember];

				string currGroupBy = null;

				if ( innerList.Count > currentListIndex )
				{
					if ( pdPrevGroupBy != null )
						prevGroupByValue = pdPrevGroupBy.GetValue(innerList[currentListIndex]).ToString();

					BindTreeNode myFirstNode = null;
					object currObject = null;

					while ( (currentListIndex < innerList.Count) &&  
						(pdPrevGroupBy != null) &&
						(pdPrevGroupBy.GetValue(innerList[currentListIndex]).ToString() == prevGroupByValue) )
					{
						currObject = innerList[currentListIndex];
						if ( pdGroupBy.GetValue (currObject).ToString() != currGroupBy )
						{
							currGroupBy = pdGroupBy.GetValue(currObject).ToString();

							myFirstNode = new BindTreeNode (currGroup.Name, 
								pdDisplay.GetValue (currObject).ToString(),
								currObject,
								pdValue.GetValue(innerList[currentListIndex]),
								currGroup.ImageIndex,
								currGroup.SelectedImageIndex,
								currentListIndex);

                            if (m_nodecontextmenustrip != null)
                                myFirstNode.ContextMenuStrip = m_nodecontextmenustrip;

							currNodes.Add( (TreeNode) myFirstNode );
						}
						else
							AddNodes(currGroupIndex, ref currentListIndex, myFirstNode.Nodes, currGroup.GroupBy);
					}
 				}
			}
			else
			{
				BindTreeNode myNewLeafNode;
				object currObject = this.m_currencyManager.List[currentListIndex];
            
				if ( (this.DisplayMember != null) && (this.ValueMember != null) &&
					 (this.DisplayMember != "") && (this.ValueMember != "") )
				{
					PropertyDescriptor pdDisplayloc = 
						this.m_currencyManager.GetItemProperties()[this.DisplayMember];
					PropertyDescriptor pdValueloc = 
						this.m_currencyManager.GetItemProperties()[this.ValueMember];

					myNewLeafNode = new BindTreeNode (this.Tag == null ? "" : this.Tag.ToString(), 
						pdDisplayloc.GetValue(currObject).ToString(), 
						currObject,
						pdValueloc.GetValue(currObject), 
						currentListIndex);
				}
				else
					myNewLeafNode = new BindTreeNode ("", currentListIndex.ToString(), 
						currObject,
						currObject, 
						this.ImageIndex, this.SelectedImageIndex,
						currentListIndex);

                if (m_nodecontextmenustrip != null)
                    myNewLeafNode.ContextMenuStrip = m_nodecontextmenustrip;					

				currNodes.Add( (TreeNode) myNewLeafNode);
				currentListIndex += 1;
			}
		}

		#endregion

		#region Groups

		public void RemoveGroup(Group group)
		{
			if ( treeGroups.Contains (group) )
			{
				treeGroups.Remove(group);
				if ( this.AutoBuildTree )
					BuildTree();
			}
		}

		public void RemoveGroup (string groupName)
		{
			foreach (Group group in this.treeGroups)
			{
				if ( group.Name == groupName )
				{
					RemoveGroup (group);
					return;
				}
			}
		}
    
		public void RemoveAllGroups ()
		{
			this.treeGroups.Clear();
			if ( this.AutoBuildTree )
				this.BuildTree();
		}

		public void AddGroup(Group group)
		{
			try
			{
				treeGroups.Add(group);
				if ( this.AutoBuildTree )
					this.BuildTree();
			}
			catch (NotSupportedException e)
			{
				MessageBox.Show (e.Message);
			}
			catch (System.Exception e)
			{
				throw e;
			}
		}

		public void AddGroup(String name, String groupBy, String displayMember,
			String valueMember, int imageIndex, int selectedImageIndex)
		{
			Group myNewGroup = new Group (name, groupBy, displayMember, valueMember, 
				imageIndex, selectedImageIndex);
			this.AddGroup (myNewGroup);
		}
    
		public Group[] GetGroups()
		{
			return ( (Group[]) treeGroups.ToArray (Type.GetType("Group")));
		}
    
		#endregion

		public void SetLeafData(String name, String displayMember, String valueMember, int imageIndex, int selectedImageIndex)
		{
			this.Tag = name;
			this.DisplayMember = displayMember;
			this.ValueMember = valueMember;
			this.ImageIndex = imageIndex;
			this.SelectedImageIndex = selectedImageIndex;
		}

		public bool IsLeafNode (TreeNode node)
		{
			return (node.Nodes.Count == 0);
		}

		#region Keeping Everything In Sync

		public TreeNode FindNodeByValue (object value)
		{
			return FindNodeByValue (value, this.Nodes);
		}

		public TreeNode FindNodeByValue (object Value, TreeNodeCollection nodesToSearch)
		{
			int i = 0;
			TreeNode currNode;
			BindTreeNode leafNode;

			while ( i < nodesToSearch.Count )
			{
				currNode = nodesToSearch[i];
				i ++;
				if ( currNode.LastNode == null )
				{
					leafNode = (BindTreeNode) currNode;
					if ( leafNode.Value.ToString() == Value.ToString() )
						return currNode;
				}
				else
				{
					currNode = FindNodeByValue (Value, currNode.Nodes);
					if ( currNode != null )
						return currNode;
				}
			}

			return null;
		}
			
		private TreeNode FindNodeByPosition (int posIndex)
		{
			return FindNodeByPosition (posIndex, this.Nodes);
		}

		private TreeNode FindNodeByPosition (int posIndex, TreeNodeCollection nodesToSearch)
		{
			int i=0;
			TreeNode currNode;
			BindTreeNode leafNode;

			while ( i < nodesToSearch.Count )
			{
				currNode = nodesToSearch [i];
				i++;
				if ( currNode.Nodes.Count == 0 )
				{
					leafNode = (BindTreeNode)currNode;
					if ( leafNode.Position == posIndex )
					{
						return currNode;
					}
					else
					{
						currNode = FindNodeByPosition (posIndex, currNode.Nodes);
						if ( currNode != null )
							return currNode;
					}
				}
			}
			return null;
		}
		
		protected override void OnAfterSelect(TreeViewEventArgs e)
		{
			BindTreeNode leafNode = (BindTreeNode)e.Node;

			if (leafNode != null)
			{
				if ( this.m_currencyManager.Position != leafNode.Position )
					this.m_currencyManager.Position = leafNode.Position;

                m_selecteditem = leafNode.Item;
            }

            // TODO:  Add MyTreeViewCtrl.OnAfterSelect implementation
			base.OnAfterSelect(e);
		}

		#endregion
	}

	public class Group
	{
		private String groupName;
		private String groupByMember;
		private String groupByDisplayMember;																   
		private String groupByValueMember;

		private int groupImageIndex;
		private int groupSelectedImageIndex;

		public Group (String name, String groupBy, String displayMember,
			String valueMember, int imageIndex, int selectedImageIndex)
		{
			this.ImageIndex = imageIndex;
			this.Name = name;
			this.GroupBy = groupBy;
			this.DisplayMember = displayMember;
			this.ValueMember = valueMember;
			this.SelectedImageIndex = selectedImageIndex;
		}

		public Group (String name, String groupBy, String displayMember,
			String valueMember, int imageIndex) :
			this (name, groupBy, displayMember, valueMember, imageIndex, imageIndex)
		{
		}
		
		public Group (String name, String groupBy) :
			this (name, groupBy, groupBy, groupBy, -1, -1)
		{
		}
		
		public int SelectedImageIndex
		{
			get { return groupSelectedImageIndex; }
			set { groupSelectedImageIndex = value; }		
		}

		public int ImageIndex
		{
			get { return groupImageIndex; }
            set { groupImageIndex = value; }
		}
			
		public String Name
		{
			get { return groupName; }
			set { groupName = value; }
		}

		public String GroupBy
		{
			get { return groupByMember; }
            set { groupByMember = value; }
		}

		public String DisplayMember
		{
			get { return groupByDisplayMember; }
			set { groupByDisplayMember = value; }
		}

		public String ValueMember
		{
			get { return groupByValueMember; }
			set { groupByValueMember = value; }
		}
	}

	public class BindTreeNode : TreeNode
	{
		private String m_groupName;
		private object m_value;
		private object m_item;
		private int m_position;

		public BindTreeNode ()
		{
		}

		public BindTreeNode (String groupName, String text, object item, object value, 
			int imageIndex, int selectedImgIndex, int position)
		{
			this.GroupName = groupName;
			this.Text = text;
			this.Item = item;
			this.Value = value;
			this.ImageIndex = imageIndex;
			this.SelectedImageIndex = selectedImgIndex;
			this.m_position = position;
		}

		public BindTreeNode (String groupName, String text, object item, object value, int position)
		{
			this.GroupName = groupName;
			this.Text = text;
			this.Item = item;
			this.Value = value;
			this.m_position = position;
		}

		public String GroupName
		{
			get { return m_groupName; }
			set { this.m_groupName = value; }
		}

		public object Item
		{
			get { return m_item; }
			set { m_item = value; }
		}

		public object Value
		{
			get { return m_value; }
			set { m_value = value; }
		}

		public int Position
		{
			get { return m_position; }
		}
	}
}

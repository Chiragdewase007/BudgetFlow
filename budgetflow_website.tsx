import React, { useState, useEffect } from 'react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, PieChart, Pie, Cell, LineChart, Line } from 'recharts';
import { DollarSign, TrendingUp, Users, Clock, CheckCircle, AlertTriangle, Settings, LogOut, Menu, X } from 'lucide-react';

const BudgetFlowPro = () => {
  const [activeTab, setActiveTab] = useState('dashboard');
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [user] = useState({ name: 'John Smith', role: 'Finance Manager', avatar: 'JS' });

  // Sample data
  const budgetData = [
    { name: 'Q1', approved: 85000, spent: 78000, remaining: 7000 },
    { name: 'Q2', approved: 92000, spent: 85000, remaining: 7000 },
    { name: 'Q3', approved: 88000, spent: 82000, remaining: 6000 },
    { name: 'Q4', approved: 95000, spent: 87000, remaining: 8000 }
  ];

  const departmentSpending = [
    { name: 'Engineering', value: 245000, color: '#8884d8' },
    { name: 'Marketing', value: 180000, color: '#82ca9d' },
    { name: 'Sales', value: 160000, color: '#ffc658' },
    { name: 'Operations', value: 120000, color: '#ff7300' },
    { name: 'HR', value: 80000, color: '#00ff88' }
  ];

  const pendingApprovals = [
    { id: 1, department: 'Engineering', amount: 45000, submittedBy: 'Alice Johnson', status: 'pending', urgency: 'high' },
    { id: 2, department: 'Marketing', amount: 28000, submittedBy: 'Bob Wilson', status: 'pending', urgency: 'medium' },
    { id: 3, department: 'Sales', amount: 35000, submittedBy: 'Carol Davis', status: 'under_review', urgency: 'low' }
  ];

  const recentTimesheets = [
    { employee: 'David Chen', project: 'Web Redesign', hours: 8, cost: 640 },
    { employee: 'Emma Rodriguez', project: 'Mobile App', hours: 6.5, cost: 520 },
    { employee: 'Frank Miller', project: 'Database Migration', hours: 7, cost: 560 }
  ];

  const Sidebar = () => (
    <div className={`fixed inset-y-0 left-0 z-50 w-64 bg-gray-900 transform transition-transform duration-300 ease-in-out ${sidebarOpen ? 'translate-x-0' : '-translate-x-full'} lg:translate-x-0 lg:static lg:inset-0`}>
      <div className="flex items-center justify-between h-16 px-6 bg-gray-800">
        <h1 className="text-xl font-bold text-white">BudgetFlow Pro</h1>
        <button onClick={() => setSidebarOpen(false)} className="lg:hidden text-white">
          <X size={24} />
        </button>
      </div>
      <nav className="mt-8">
        {[
          { id: 'dashboard', label: 'Dashboard', icon: BarChart },
          { id: 'budgets', label: 'Budget Management', icon: DollarSign },
          { id: 'approvals', label: 'Approvals', icon: CheckCircle },
          { id: 'timesheets', label: 'Timesheets', icon: Clock },
          { id: 'reports', label: 'Reports', icon: TrendingUp },
          { id: 'users', label: 'User Management', icon: Users }
        ].map(item => {
          const Icon = item.icon;
          return (
            <a
              key={item.id}
              onClick={() => { setActiveTab(item.id); setSidebarOpen(false); }}
              className={`flex items-center px-6 py-3 text-gray-300 hover:bg-gray-700 hover:text-white cursor-pointer transition-colors ${activeTab === item.id ? 'bg-gray-700 text-white border-r-2 border-blue-500' : ''}`}
            >
              <Icon size={20} className="mr-3" />
              {item.label}
            </a>
          );
        })}
      </nav>
    </div>
  );

  const Header = () => (
    <header className="bg-white shadow-sm border-b border-gray-200 px-6 py-4">
      <div className="flex items-center justify-between">
        <div className="flex items-center">
          <button onClick={() => setSidebarOpen(true)} className="lg:hidden mr-4 text-gray-600">
            <Menu size={24} />
          </button>
          <h2 className="text-2xl font-semibold text-gray-800 capitalize">{activeTab.replace('_', ' ')}</h2>
        </div>
        <div className="flex items-center space-x-4">
          <div className="text-right">
            <p className="text-sm font-medium text-gray-900">{user.name}</p>
            <p className="text-sm text-gray-500">{user.role}</p>
          </div>
          <div className="w-10 h-10 bg-blue-500 rounded-full flex items-center justify-center text-white font-semibold">
            {user.avatar}
          </div>
          <button className="text-gray-400 hover:text-gray-600">
            <Settings size={20} />
          </button>
          <button className="text-gray-400 hover:text-gray-600">
            <LogOut size={20} />
          </button>
        </div>
      </div>
    </header>
  );

  const StatCard = ({ title, value, change, icon: Icon, color = 'blue' }) => (
    <div className="bg-white rounded-lg shadow p-6">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm font-medium text-gray-600">{title}</p>
          <p className="text-3xl font-bold text-gray-900">{value}</p>
          <p className={`text-sm ${change >= 0 ? 'text-green-600' : 'text-red-600'}`}>
            {change >= 0 ? '+' : ''}{change}% vs last month
          </p>
        </div>
        <div className={`w-12 h-12 bg-${color}-100 rounded-lg flex items-center justify-center`}>
          <Icon className={`w-6 h-6 text-${color}-600`} />
        </div>
      </div>
    </div>
  );

  const Dashboard = () => (
    <div className="space-y-6">
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <StatCard title="Total Budget" value="$360K" change={12} icon={DollarSign} color="blue" />
        <StatCard title="Spent This Month" value="$85K" change={-3} icon={TrendingUp} color="green" />
        <StatCard title="Pending Approvals" value="12" change={25} icon={AlertTriangle} color="yellow" />
        <StatCard title="Active Projects" value="8" change={0} icon={CheckCircle} color="purple" />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-white rounded-lg shadow p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Quarterly Budget Overview</h3>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={budgetData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="name" />
              <YAxis />
              <Tooltip formatter={(value) => [`$${value.toLocaleString()}`, '']} />
              <Bar dataKey="approved" fill="#8884d8" name="Approved" />
              <Bar dataKey="spent" fill="#82ca9d" name="Spent" />
            </BarChart>
          </ResponsiveContainer>
        </div>

        <div className="bg-white rounded-lg shadow p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Department Spending</h3>
          <ResponsiveContainer width="100%" height={300}>
            <PieChart>
              <Pie
                data={departmentSpending}
                cx="50%"
                cy="50%"
                outerRadius={100}
                fill="#8884d8"
                dataKey="value"
                label={({ name, value }) => `${name}: $${value.toLocaleString()}`}
              >
                {departmentSpending.map((entry, index) => (
                  <Cell key={`cell-${index}`} fill={entry.color} />
                ))}
              </Pie>
              <Tooltip formatter={(value) => [`$${value.toLocaleString()}`, 'Amount']} />
            </PieChart>
          </ResponsiveContainer>
        </div>
      </div>

      <div className="bg-white rounded-lg shadow p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Recent Activity</h3>
        <div className="space-y-4">
          {pendingApprovals.slice(0, 3).map(approval => (
            <div key={approval.id} className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
              <div>
                <p className="font-medium text-gray-900">{approval.department} Budget Request</p>
                <p className="text-sm text-gray-600">Submitted by {approval.submittedBy}</p>
              </div>
              <div className="text-right">
                <p className="font-semibold text-gray-900">${approval.amount.toLocaleString()}</p>
                <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${
                  approval.urgency === 'high' ? 'bg-red-100 text-red-800' :
                  approval.urgency === 'medium' ? 'bg-yellow-100 text-yellow-800' :
                  'bg-green-100 text-green-800'
                }`}>
                  {approval.urgency} priority
                </span>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );

  const BudgetManagement = () => (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h3 className="text-lg font-semibold text-gray-900">Budget Management</h3>
        <button className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition-colors">
          Create New Budget
        </button>
      </div>

      <div className="bg-white rounded-lg shadow overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Department</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Budget</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Spent</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Remaining</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {departmentSpending.map((dept, index) => {
              const spent = dept.value;
              const budget = spent * 1.2;
              const remaining = budget - spent;
              return (
                <tr key={index}>
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{dept.name}</td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">${budget.toLocaleString()}</td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">${spent.toLocaleString()}</td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">${remaining.toLocaleString()}</td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className="inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-green-100 text-green-800">
                      Active
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                    <button className="text-blue-600 hover:text-blue-900 mr-4">Edit</button>
                    <button className="text-gray-600 hover:text-gray-900">View</button>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </div>
  );

  const Approvals = () => (
    <div className="space-y-6">
      <h3 className="text-lg font-semibold text-gray-900">Pending Approvals</h3>
      
      <div className="space-y-4">
        {pendingApprovals.map(approval => (
          <div key={approval.id} className="bg-white rounded-lg shadow p-6">
            <div className="flex items-center justify-between mb-4">
              <div>
                <h4 className="text-lg font-medium text-gray-900">{approval.department} Budget Request</h4>
                <p className="text-sm text-gray-600">Submitted by {approval.submittedBy}</p>
              </div>
              <div className="text-right">
                <p className="text-2xl font-bold text-gray-900">${approval.amount.toLocaleString()}</p>
                <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${
                  approval.urgency === 'high' ? 'bg-red-100 text-red-800' :
                  approval.urgency === 'medium' ? 'bg-yellow-100 text-yellow-800' :
                  'bg-green-100 text-green-800'
                }`}>
                  {approval.urgency} priority
                </span>
              </div>
            </div>
            <div className="flex space-x-3">
              <button className="flex-1 bg-green-600 text-white px-4 py-2 rounded-lg hover:bg-green-700 transition-colors">
                Approve
              </button>
              <button className="flex-1 bg-red-600 text-white px-4 py-2 rounded-lg hover:bg-red-700 transition-colors">
                Reject
              </button>
              <button className="flex-1 bg-gray-600 text-white px-4 py-2 rounded-lg hover:bg-gray-700 transition-colors">
                Request More Info
              </button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );

  const Timesheets = () => (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h3 className="text-lg font-semibold text-gray-900">Timesheet Management</h3>
        <button className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition-colors">
          Add Time Entry
        </button>
      </div>

      <div className="bg-white rounded-lg shadow overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Employee</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Project</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Hours</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Cost</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {recentTimesheets.map((entry, index) => (
              <tr key={index}>
                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{entry.employee}</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{entry.project}</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{entry.hours}h</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">${entry.cost}</td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <span className="inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-green-100 text-green-800">
                    Approved
                  </span>
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                  <button className="text-blue-600 hover:text-blue-900 mr-4">Edit</button>
                  <button className="text-gray-600 hover:text-gray-900">Delete</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );

  const renderContent = () => {
    switch (activeTab) {
      case 'dashboard': return <Dashboard />;
      case 'budgets': return <BudgetManagement />;
      case 'approvals': return <Approvals />;
      case 'timesheets': return <Timesheets />;
      case 'reports': return <div className="text-center py-12"><p className="text-gray-500">Reports module coming soon...</p></div>;
      case 'users': return <div className="text-center py-12"><p className="text-gray-500">User management module coming soon...</p></div>;
      default: return <Dashboard />;
    }
  };

  return (
    <div className="flex h-screen bg-gray-100">
      <Sidebar />
      <div className="flex-1 flex flex-col overflow-hidden">
        <Header />
        <main className="flex-1 overflow-x-hidden overflow-y-auto bg-gray-100 p-6">
          {renderContent()}
        </main>
      </div>
      {sidebarOpen && (
        <div className="fixed inset-0 z-40 bg-black bg-opacity-50 lg:hidden" onClick={() => setSidebarOpen(false)} />
      )}
    </div>
  );
};

export default BudgetFlowPro;
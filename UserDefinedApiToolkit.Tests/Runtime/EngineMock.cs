namespace UserDefinedApiToolkit.Tests.Runtime
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Utils.DOM.UnitTesting;

	internal class EngineMock : IEngine
	{
		private readonly DomSLNetMessageHandler _messageHandler;
		private readonly IConnection _connection;

		public EngineMock()
		{
			_messageHandler = new DomSLNetMessageHandler();
			_connection = new DomConnectionMock(_messageHandler);
		}

		public bool IsInteractive { get; } = false;
		public SLTicketingGateway TicketingGateway { get; }
		public SLProfileManager ProfileManager { get; }
		public string UserCookie { get; } = "This is a mock";
		public string TriggeredByName { get; } = "UserDefinedApiToolkit.Tests";
		public string UserDisplayName { get; } = "UserDefinedApiToolkit.Tests";
		public string UserLoginName { get; } = "UserDefinedApiToolkit.Tests";
		public int InstanceId { get; } = 1;
		public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(15);

		public void AcknowledgeAlarm(AlarmTreeID alarmTreeID, string comment) => throw new NotImplementedException();
		public void AcknowledgeAlarm(int dataMinerID, int elementID, int alarmID, string comment) => throw new NotImplementedException();
		public void AcknowledgeAlarm(int dataMinerID, int alarmID, string comment) => throw new NotImplementedException();
		public void AddError(string error) => throw new NotImplementedException();
		public void AddOrUpdateScriptOutput(string key, string value) => throw new NotImplementedException();
		public void AddScriptOutput(string key, string value) => throw new NotImplementedException();
		public void AddSingularJsonOutput(string value) => throw new NotImplementedException();
		public void ClearScriptOutput(string key) => throw new NotImplementedException();
		public void ClearScriptResult() => throw new NotImplementedException();
		public ScriptDummy CreateExtraDummy(int dataMinerID, int elementID, string key) => throw new NotImplementedException();
		public ScriptDummy CreateExtraDummy(int dataMinerID, int elementID) => throw new NotImplementedException();
		public void ExitFail(string reason) => throw new NotImplementedException();
		public void ExitSuccess(string reason) => throw new NotImplementedException();
		public Element FindElement(string name) => throw new NotImplementedException();
		public Element FindElement(int dmaID, int elementID) => throw new NotImplementedException();
		public Element FindElementByKey(string key) => throw new NotImplementedException();
		public Element[] FindElements(ElementFilter filter) => throw new NotImplementedException();
		public Element[] FindElementsByName(string nameFilter) => throw new NotImplementedException();
		public Element[] FindElementsByProtocol(string name) => throw new NotImplementedException();
		public Element[] FindElementsByProtocol(string name, string version) => throw new NotImplementedException();
		public Element[] FindElementsInView(int viewID, string protocolName, string protocolVersion) => throw new NotImplementedException();
		public Element[] FindElementsInView(string viewName, string protocolName, string protocolVersion) => throw new NotImplementedException();
		public Element[] FindElementsInView(int viewID) => throw new NotImplementedException();
		public Element[] FindElementsInView(string viewName) => throw new NotImplementedException();
		public bool FindInteractiveClient(string message, int timeoutTime, string allowedGroups, AutomationScriptAttachOptions options) => throw new NotImplementedException();
		public bool FindInteractiveClient(string message, int timeoutTime, string allowedGroups) => throw new NotImplementedException();
		public bool FindInteractiveClient(string message, int timeoutTime) => throw new NotImplementedException();
		public RedundancyGroup FindRedundancyGroup(string name) => throw new NotImplementedException();
		public RedundancyGroup FindRedundancyGroup(int dmaID, int groupID) => throw new NotImplementedException();
		public RedundancyGroup FindRedundancyGroupByKey(string key) => throw new NotImplementedException();
		public RedundancyGroup[] FindRedundancyGroups(RedundancyGroupFilter filter) => throw new NotImplementedException();
		public RedundancyGroup[] FindRedundancyGroupsByName(string nameFilter) => throw new NotImplementedException();
		public RedundancyGroup[] FindRedundancyGroupsInView(int viewID) => throw new NotImplementedException();
		public RedundancyGroup[] FindRedundancyGroupsInView(string viewName) => throw new NotImplementedException();
		public Service FindService(string name) => throw new NotImplementedException();
		public Service FindService(int dmaID, int serviceID) => throw new NotImplementedException();
		public Service FindServiceByKey(string key) => throw new NotImplementedException();
		public Service[] FindServices(ServiceFilter filter) => throw new NotImplementedException();
		public Service[] FindServicesByName(string nameFilter) => throw new NotImplementedException();
		public Service[] FindServicesInView(int viewID) => throw new NotImplementedException();
		public Service[] FindServicesInView(string viewName) => throw new NotImplementedException();
		public void GenerateInformation(string text) => Console.WriteLine($"Information: {text}");
		public string GetAlarmProperty(AlarmID alarmID, string propertyName) => throw new NotImplementedException();
		public string GetAlarmProperty(int dataMinerID, int elementID, int alarmID, string propertyName) => throw new NotImplementedException();
		public string GetAlarmProperty(int dataMinerID, int alarmID, string propertyName) => throw new NotImplementedException();
		public ScriptDummy GetDummy(string name) => throw new NotImplementedException();
		public ScriptDummy GetDummy(int id) => throw new NotImplementedException();
		public ScriptMemory GetMemory(string name) => throw new NotImplementedException();
		public ScriptMemory GetMemory(int id) => throw new NotImplementedException();
		public string GetScriptOutput(string key) => throw new NotImplementedException();
		public ScriptParam GetScriptParam(string name) => throw new NotImplementedException();
		public ScriptParam GetScriptParam(int id) => throw new NotImplementedException();
		public Dictionary<string, string> GetScriptResult() => throw new NotImplementedException();
		public IConnection GetUserConnection() => _connection;
		public void HideUI() => throw new NotImplementedException();
		public void KeepAlive() => throw new NotImplementedException();
		public double LoadDoubleValue(string name) => throw new NotImplementedException();
		public string LoadStringValue(string name) => throw new NotImplementedException();
		public object LoadValue(string name) => throw new NotImplementedException();
		public void Log(string message, LogType type, int logLevel, string method) => Console.WriteLine($"Log: {message}, Type: {type}, LogLevel: {logLevel}, Method: {method}");
		public void Log(string message, LogType type, int logLevel) => Console.WriteLine($"Log: {message}, Type: {type}, LogLevel: {logLevel}");
		public void Log(string message) => Console.WriteLine($"Log: {message}");
		public MailReportOptions PrepareMailReport(string mailReport) => throw new NotImplementedException();
		public SubScriptOptions PrepareSubScript(string scriptName) => throw new NotImplementedException();
		public UIResults RunClientProgram(string applicationPath, bool waitForCompletion) => throw new NotImplementedException();
		public UIResults RunClientProgram(string applicationPath) => throw new NotImplementedException();
		public UIResults RunClientProgram(string applicationPath, string arguments) => throw new NotImplementedException();
		public UIResults RunClientProgram(string applicationPath, string arguments, bool waitForCompletion) => throw new NotImplementedException();
		public void SaveValue(string name, double value) => throw new NotImplementedException();
		public void SaveValue(string name, string value) => throw new NotImplementedException();
		public void SendEmail(EmailOptions options) => throw new NotImplementedException();
		public void SendEmail(string message, string title, string to) => throw new NotImplementedException();
		public void SendPager(PagerOptions options) => throw new NotImplementedException();
		public void SendPager(string message, string to) => throw new NotImplementedException();
		public void SendReport(MailReportOptions options) => throw new NotImplementedException();
		public DMSMessage[] SendSLNetMessage(DMSMessage message) => _connection.HandleMessage(message);
		public DMSMessage[] SendSLNetMessages(DMSMessage[] A_0) => _connection.HandleMessages(A_0);
		public DMSMessage SendSLNetSingleResponseMessage(DMSMessage message) => _connection.HandleSingleResponseMessage(message);
		public void SendSms(SmsOptions options) => throw new NotImplementedException();
		public void SendSms(string message, string to) => throw new NotImplementedException();
		public void SetAlarmProperties(AlarmTreeID alarmTreeID, string[] propertyNames, string[] propertyValues) => throw new NotImplementedException();
		public void SetAlarmProperties(int dataMinerID, int elementID, int alarmID, string[] propertyNames, string[] propertyValues) => throw new NotImplementedException();
		public void SetAlarmProperties(int dataMinerID, int alarmID, string[] propertyNames, string[] propertyValues) => throw new NotImplementedException();
		public void SetAlarmProperty(AlarmTreeID alarmTreeID, string propertyName, string propertyValue) => throw new NotImplementedException();
		public void SetAlarmProperty(int dataMinerID, int elementID, int alarmID, string propertyName, string propertyValue) => throw new NotImplementedException();
		public void SetAlarmProperty(int dataMinerID, int alarmID, string propertyName, string propertyValue) => throw new NotImplementedException();
		public void SetFlag(RunTimeFlags flag) => throw new NotImplementedException();
		public void ShowProgress(string uiData) => throw new NotImplementedException();
		public UIResults ShowUI(UIBuilder uiBuilder) => throw new NotImplementedException();
		public UIResults ShowUI(string uiData, bool requireResponse) => throw new NotImplementedException();
		public UIResults ShowUI(string uiData) => throw new NotImplementedException();
		public void Sleep(int timeInMilliseconds) => Thread.Sleep(timeInMilliseconds);
		public void UnSetFlag(RunTimeFlags flag) => throw new NotImplementedException();
	}
}

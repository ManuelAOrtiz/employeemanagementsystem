# employeemanagementsystem
This project is about learning ASP.NET and its MVC system.

1.) Download the repository.

2.) Start Visual Studios.

3.) Open EmployeeManagement.sln
![alt text](readme/first.png)
4.) Go to View menu on the top toolbar then select the Server Explorer on the dropdown menu.
![alt text](readme/second.png)
5.) Depending on the version of Visual Studio you are running what you need to do next may change.
  - Right Click Data Connection on the drop down menu select Add Connection
  ![alt text](readme/third.png)
  - For the Data source select: Microsoft SQL Server Database File
  ![alt text](readme/Fourth.png)
  - Click ok
  - Under Database File Name select browse: Enter into the DatabaseTable folder by double clicking it.
  - Select trial.mdf then okay button.
  ![alt text](readme/fifth.png)
  
6.) Right click trial.mdf under Data Connection and select properities

7.) Double click the connection string value and copy.
![alt text](readme/sixth.png)


8.) Open up Web.config and go to line 69
  - <add name="Employee" connectionString="(paste your connection string here)"
  
  ![alt text](readme/seventh.png)
 9.) Run the project.
  ![alt text](readme/eight.png)
 All passwords are currently 123456.
 Users:
  1.)admin -
      to test all the features on the site.
  2.)silver -
      to test what would be a hr level user.
  3.)manny#R9BY - 
      to test what an employee level would have access to.
  4.)guest -
      a user generated but not attached to an employee.

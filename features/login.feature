@login
Feature: User Login
  As a user I want to log in so that I can access the application

  Background:
    Given the application is launched
    And PowerScribe is connected

  # @alpha @beta
  Scenario: Successful login with valid credentials
    When I login to PowerScribe with username "andy.murray" and password "Qurelungs123!"
    Then disconnect button should not be visibled on widget
    # Then I should be redirected to the dashboard
  
  # @alpha
  # Examples:
  #   | username      | password          |
  #   | andy.murray    | Qurelungs123!     |

  # @beta
  # Examples:
  #   | username       | password          |
  #   | andy.murray    | Qurelungs123!     |


  #Scenario: Failed login with invalid credentials
   # When I login to PowerScribe with username "invalid_user" and password "wrong_pass"
   # Then I should see an error message "Invalid credentials"

  #Scenario: Login blocked when fields are empty
    #When I click the login button
    #Then I should see a validation error on the login form

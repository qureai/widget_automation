@dashboard
Feature: Dashboard
  As a logged-in user I want to see dashboard widgets so I can monitor key metrics

  Background:
    Given the application is launched
    And I am logged in as "admin" with password "password123"

  Scenario: Dashboard loads with all widgets visible
    Then the dashboard header should be visible
    And the metrics panel should be visible
    And the activity feed should be visible

  Scenario: Navigating to a sub-section from the dashboard
    When I click on "Reports" in the navigation menu
    Then I should be on the reports page

  Scenario: Refreshing dashboard data
    When I click the refresh button
    Then the metrics panel should display updated data

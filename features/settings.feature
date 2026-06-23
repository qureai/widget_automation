@settings
Feature: Settings
  As a logged-in user I want to configure app settings so the app behaves as I prefer

  Background:
    Given the application is launched
    And I am logged in as "admin" with password "password123"
    And I navigate to the settings page

  Scenario: Changing the display theme
    When I select "Dark" from the theme dropdown
    And I click the save settings button
    Then a success notification should appear
    And the application theme should be "Dark"

  Scenario: Updating user profile information
    When I update the display name to "Test User"
    And I click the save settings button
    Then a success notification should appear

  Scenario: Settings persist after restart
    When I select "Dark" from the theme dropdown
    And I click the save settings button
    And the application restarts
    Then the theme should still be "Dark"

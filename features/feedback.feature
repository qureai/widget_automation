@feedback
Feature: Feedback Buttons
  As a user I want to see feedback buttons on the widget after opening an accession on PowerScribe

  Background:
    Given the application is launched
    And PowerScribe is connected

  Scenario: Feedback buttons are visible after opening an accession
    When I login to PowerScribe with username "andy.murray" and password "Qurelungs123!"
    And I open accession "Segmed_854167581" on PowerScribe
    Then the thumbs up button should be visible
    And the thumbs down button should be visible

Scenario: Before submitting feedback, the buttons should be in default state
    When I login to PowerScribe with username "andy.murray" and password "Qurelungs123!"
    And I open accession "Segmed_854167581" on PowerScribe
    Then the thumbs up button should be in default state
    And the thumbs down button should be in default state


Scenario: Submitting thumbs up feedback
    When I login to PowerScribe with username "andy.murray" and password "Qurelungs123!"
    And I open accession "Segmed_854167581" on PowerScribe
    And I click the thumbs up button
    Then the message should populate as submitted
    Then the thumbs up button should be in active state
    And the thumbs down button should be in default state
    
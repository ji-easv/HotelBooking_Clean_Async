Feature: Book a room
Room booking feature for a hotel booking system

    Background: Fully booked period is from 10 days from today to 20 days from today

    Scenario Outline: Successfully book a free room
        Given there is at least one available room
        And a customer wants to book a room from <startDateOffset> days from today to <endDateOffset> days from today
        When the customer submits a booking request
        Then the booking should be created successfully
        And the room should be marked as booked for those dates

    Examples:
      | startDateOffset | endDateOffset |
      | 1               | 9             |
      | 21              | 22            |

    Scenario Outline: : Attempt to book when all rooms are occupied
        Given all rooms are already booked for the requested dates
        And a customer wants to book a room from <startDateOffset> days from today to <endDateOffset> days from today
        When the customer submits a booking request
        Then the booking should be rejected
        And the customer should receive a message that no rooms are available

    Examples:
      | startDateOffset | endDateOffset |
      | 9               | 21            |
      | 9               | 10            |
      | 20              | 21            |
      | 10              | 20            |
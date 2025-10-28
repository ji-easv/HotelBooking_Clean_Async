Feature: Book a room

Room booking feature for a hotel booking system

    @mytag
    Scenario: Successfully book a free room
        Given there is at least one available room
        And a customer wants to book a room from <startDateOffset> days from today to <endDateOffset> days from today
        When the customer submits a booking request
        Then the booking should be created successfully
        And the room should be marked as booked for those dates

    Examples:
      | startDateOffset | endDateOffset |
      | 1               | 2             |
      | 7               | 9             |
      | 21              | 25            |

    @mytag
    Scenario: Attempt to book when all rooms are occupied
        Given all rooms are already booked for the requested dates
        And a customer wants to book a room from <startDateOffset> days from today to <endDateOffset> days from today
        When the customer submits a booking request
        Then the booking should be rejected
        And the customer should receive a message that no rooms are available

    Examples:
      | startDateOffset | endDateOffset |
      | 10              | 20            |
      | 15              | 17            |

@feature-tag
Feature: Feature1

Background:
    Given this is feature level background step

@scenario-tag1
Scenario: this is scenario
	Given this is given step
	When this is when step with '1' argument an 'text' argument
    | Name | Value   |
    | Data | Contnet |
    When this is second when step with date '01/01/2020' argument
        And this is when step with async task
	Then this is then step
    When step with model transformation
    | Key | Value |
    | 1   | One   |
    | 2   | Two   |

Scenario Outline: this is scenario outline
	When this is when step with with argument '<number>' and value '<text>'
    
    Examples: 
    | number | text   |
    | 1      | value1 |
    | 2      | value2 |

    Examples: 
    | number | text   |
    | 3      | value3 |
    | 4      | value4 |

Rule: this is rule

    Background: 
        Given this is rule level background step

    @scenario-tag
    Scenario: this is scenario from rule
	    Given this is given step

    Scenario Outline: this is scenario outline from rule
        When this is when step with with argument '<number>' and value '<text>'

        Examples: 
        | number | text   |
        | 1      | value1 |
        | 2      | value2 |

        Examples: 
        | number | text   |
        | 3      | value3 |
        | 4      | value4 |
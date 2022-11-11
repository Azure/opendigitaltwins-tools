param location string
param dashboardName string = 'Microsoft SmartPlaces Facilities Topology'
param appInsightsName string

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: appInsightsName
}

resource dashboard 'Microsoft.Portal/dashboards@2020-09-01-preview' = {
  name: '${appInsightsName}-dashboard'
  location: location
  tags: {
    'hidden-title': dashboardName
  }
  properties: {
    lenses: [
      {
        order: 0
        parts: [
          {
            position: {
              x: 0
              y: 0
              rowSpan: 1
              colSpan: 24
            }
            metadata: {
              inputs: []
              type: 'Extension/HubsExtension/PartType/MarkdownPart'
              settings: {
                content: {
                  #disable-next-line BCP037
                  content: '# Ingestion Metrics'
                  #disable-next-line BCP037
                  markdownSource: 1
                }
              }
            }
          }
          {
            position: {
              x: 0
              y: 1
              rowSpan: 4
              colSpan: 6
            }
            metadata: {
              inputs: [
                {
                  name: 'options'
                  isOptional: true
                }
                {
                  name: 'sharedTimeRange'
                  isOptional: true
                }
              ]
              #disable-next-line BCP036
              type: 'Extension/HubsExtension/PartType/MonitorChartPart'
              settings: {
                content: {
                  options: {
                    chart: {
                      metrics: [
                        {
                          resourceMetadata: {
                            id: appInsights.id
                          }
                          name: 'SiteProcessed'
                          aggregationType: 1
                          namespace: 'mapped'
                          metricVisualization: {
                            displayName: 'SiteProcessed'
                            resourceDisplayName: appInsightsName
                          }
                        }
                      ]
                      title: 'Sites Ingested'
                      titleKind: 2
                      visualization: {
                        chartType: 1
                        legendVisualization: {
                          isVisible: true
                          position: 2
                          hideSubtitle: false
                        }
                        axisVisualization: {
                          x: {
                            isVisible: true
                            axisType: 2
                          }
                          y: {
                            isVisible: true
                            axisType: 1
                          }
                        }
                        disablePinning: true
                      }
                      grouping: {
                        dimension: 'site'
                        sort: 2
                        top: 10
                      }
                    }
                  }
                }
              }
            }
          }
          {
            position: {
              x: 6
              y: 1
              rowSpan: 4
              colSpan: 6
            }
            metadata: {
              inputs: [
                {
                  name: 'options'
                  isOptional: true
                }
                {
                  name: 'sharedTimeRange'
                  isOptional: true
                }
              ]
              #disable-next-line BCP036
              type: 'Extension/HubsExtension/PartType/MonitorChartPart'
              settings: {
                content: {
                  options: {
                    chart: {
                      metrics: [
                        {
                          resourceMetadata: {
                            id: appInsights.id
                          }
                          name: 'Buildings'
                          aggregationType: 1
                          namespace: 'mapped'
                          metricVisualization: {
                            displayName: 'Buildings'
                            resourceDisplayName: appInsightsName
                          }
                        }
                      ]
                      title: 'Buildings Ingested'
                      titleKind: 2
                      visualization: {
                        chartType: 1
                        legendVisualization: {
                          isVisible: true
                          position: 2
                          hideSubtitle: false
                        }
                        axisVisualization: {
                          x: {
                            isVisible: true
                            axisType: 2
                          }
                          y: {
                            isVisible: true
                            axisType: 1
                          }
                        }
                        disablePinning: true
                      }
                    }
                  }
                }
              }
            }
          }
          {
            position: {
              x: 12
              y: 1
              rowSpan: 4
              colSpan: 6
            }
            metadata: {
              inputs: [
                {
                  name: 'options'
                  isOptional: true
                }
                {
                  name: 'sharedTimeRange'
                  isOptional: true
                }
              ]
              #disable-next-line BCP036
              type: 'Extension/HubsExtension/PartType/MonitorChartPart'
              settings: {
                content: {
                  options: {
                    chart: {
                      metrics: [
                        {
                          resourceMetadata: {
                            id: appInsights.id
                          }
                          name: 'Twins'
                          aggregationType: 1
                          namespace: 'mapped'
                          metricVisualization: {
                            displayName: 'Twins'
                            resourceDisplayName: appInsightsName
                          }
                        }
                      ]
                      title: 'Twins Ingested by Building'
                      titleKind: 2
                      visualization: {
                        chartType: 1
                        legendVisualization: {
                          isVisible: true
                          position: 2
                          hideSubtitle: false
                        }
                        axisVisualization: {
                          x: {
                            isVisible: true
                            axisType: 2
                          }
                          y: {
                            isVisible: true
                            axisType: 1
                          }
                        }
                        disablePinning: true
                      }
                      grouping: {
                        dimension: 'Building'
                        sort: 2
                        top: 10
                      }
                    }
                  }
                }
              }
            }
          }
          {
            position: {
              x: 18
              y: 1
              rowSpan: 4
              colSpan: 6
            }
            metadata: {
              inputs: [
                {
                  name: 'options'
                  isOptional: true
                }
                {
                  name: 'sharedTimeRange'
                  isOptional: true
                }
              ]
              #disable-next-line BCP036
              type: 'Extension/HubsExtension/PartType/MonitorChartPart'
              settings: {
                content: {
                  options: {
                    chart: {
                      metrics: [
                        {
                          resourceMetadata: {
                            id: appInsights.id
                          }
                          name: 'Relationships'
                          aggregationType: 1
                          namespace: 'mapped'
                          metricVisualization: {
                            displayName: 'Relationships'
                            resourceDisplayName: appInsightsName
                          }
                        }
                      ]
                      title: 'Relationships Ingested by Building'
                      titleKind: 2
                      visualization: {
                        chartType: 1
                        legendVisualization: {
                          isVisible: true
                          position: 2
                          hideSubtitle: false
                        }
                        axisVisualization: {
                          x: {
                            isVisible: true
                            axisType: 2
                          }
                          y: {
                            isVisible: true
                            axisType: 1
                          }
                        }
                        disablePinning: true
                      }
                      grouping: {
                        dimension: 'Building'
                        sort: 2
                        top: 10
                      }
                    }
                  }
                }
              }
            }
          }
          {
            position: {
              x: 0
              y: 5
              rowSpan: 1
              colSpan: 24
            }
            metadata: {
              inputs: []
              type: 'Extension/HubsExtension/PartType/MarkdownPart'
              settings: {
                content: {
                  #disable-next-line BCP037
                  content: '## Twins'
                  #disable-next-line BCP037
                  markdownSource: 1
                }
              }
            }
          }
          {
            position: {
              x: 0
              y: 6
              rowSpan: 4
              colSpan: 6
            }
            metadata: {
              inputs: [
                {
                  name: 'options'
                  isOptional: true
                }
                {
                  name: 'sharedTimeRange'
                  isOptional: true
                }
              ]
              #disable-next-line BCP036
              type: 'Extension/HubsExtension/PartType/MonitorChartPart'
              settings: {
                content: {
                  options: {
                    chart: {
                      metrics: [
                        {
                          resourceMetadata: {
                            id: appInsights.id
                          }
                          name: 'Twins'
                          aggregationType: 1
                          namespace: 'azure.applicationinsights'
                          metricVisualization: {
                            displayName: 'Twins'
                            resourceDisplayName: appInsightsName
                          }
                        }
                      ]
                      title: 'Twin Creates Succeeded'
                      titleKind: 2
                      visualization: {
                        chartType: 5
                        legendVisualization: {
                          isVisible: true
                          position: 2
                          hideSubtitle: false
                        }
                        axisVisualization: {
                          x: {
                            isVisible: true
                            axisType: 2
                          }
                          y: {
                            isVisible: true
                            axisType: 1
                          }
                        }
                        disablePinning: true
                      }
                      filterCollection: {
                        filters: [
                          {
                            key: 'Action'
                            operator: 0
                            values: [
                              'create'
                            ]
                          }
                          {
                            key: 'status'
                            operator: 0
                            values: [
                              'Succeeded'
                            ]
                          }
                        ]
                      }
                      grouping: {
                        dimension: 'ModelId'
                        sort: 2
                        top: 10
                      }
                    }
                  }
                }
              }
            }
          }
          {
            position: {
              x: 6
              y: 6
              rowSpan: 4
              colSpan: 6
            }
            metadata: {
              inputs: [
                {
                  name: 'options'
                  isOptional: true
                }
                {
                  name: 'sharedTimeRange'
                  isOptional: true
                }
              ]
              #disable-next-line BCP036
              type: 'Extension/HubsExtension/PartType/MonitorChartPart'
              settings: {
                content: {
                  options: {
                    chart: {
                      metrics: [
                        {
                          resourceMetadata: {
                            id: appInsights.id
                          }
                          name: 'Twins'
                          aggregationType: 1
                          namespace: 'azure.applicationinsights'
                          metricVisualization: {
                            displayName: 'Twins'
                            resourceDisplayName: appInsightsName
                          }
                        }
                      ]
                      title: 'Twin Create/Reset Failed'
                      titleKind: 2
                      visualization: {
                        chartType: 5
                        legendVisualization: {
                          isVisible: true
                          position: 2
                          hideSubtitle: false
                        }
                        axisVisualization: {
                          x: {
                            isVisible: true
                            axisType: 2
                          }
                          y: {
                            isVisible: true
                            axisType: 1
                          }
                        }
                        disablePinning: true
                      }
                      filterCollection: {
                        filters: [
                          {
                            key: 'Action'
                            operator: 0
                            values: [
                              'create'
                            ]
                          }
                          {
                            key: 'status'
                            operator: 0
                            values: [
                              'Failed'
                            ]
                          }
                        ]
                      }
                      grouping: {
                        dimension: 'ModelId'
                        sort: 2
                        top: 10
                      }
                    }
                  }
                }
              }
            }
          }
          {
            position: {
              x: 12
              y: 6
              rowSpan: 4
              colSpan: 6
            }
            metadata: {
              inputs: [
                {
                  name: 'options'
                  isOptional: true
                }
                {
                  name: 'sharedTimeRange'
                  isOptional: true
                }
              ]
              #disable-next-line BCP036
              type: 'Extension/HubsExtension/PartType/MonitorChartPart'
              settings: {
                content: {
                  options: {
                    chart: {
                      metrics: [
                        {
                          resourceMetadata: {
                            id: appInsights.id
                          }
                          name: 'Twins'
                          aggregationType: 1
                          namespace: 'azure.applicationinsights'
                          metricVisualization: {
                            displayName: 'Twins'
                            resourceDisplayName: appInsightsName
                          }
                        }
                      ]
                      title: 'Create/Reset Twin - Throttled'
                      titleKind: 2
                      visualization: {
                        chartType: 5
                        legendVisualization: {
                          isVisible: true
                          position: 2
                          hideSubtitle: false
                        }
                        axisVisualization: {
                          x: {
                            isVisible: true
                            axisType: 2
                          }
                          y: {
                            isVisible: true
                            axisType: 1
                          }
                        }
                        disablePinning: true
                      }
                      filterCollection: {
                        filters: [
                          {
                            key: 'Action'
                            operator: 0
                            values: [
                              'create'
                            ]
                          }
                          {
                            key: 'status'
                            operator: 0
                            values: [
                              'Throttled'
                            ]
                          }
                        ]
                      }
                      grouping: {
                        dimension: 'ModelId'
                        sort: 2
                        top: 10
                      }
                    }
                  }
                }
              }
            }
          }
          {
            position: {
              x: 0
              y: 10
              rowSpan: 4
              colSpan: 6
            }
            metadata: {
              inputs: [
                {
                  name: 'options'
                  isOptional: true
                }
                {
                  name: 'sharedTimeRange'
                  isOptional: true
                }
              ]
              #disable-next-line BCP036
              type: 'Extension/HubsExtension/PartType/MonitorChartPart'
              settings: {
                content: {
                  options: {
                    chart: {
                      metrics: [
                        {
                          resourceMetadata: {
                            id: appInsights.id
                          }
                          name: 'Twins'
                          aggregationType: 1
                          namespace: 'azure.applicationinsights'
                          metricVisualization: {
                            displayName: 'Twins'
                            resourceDisplayName: appInsightsName
                          }
                        }
                      ]
                      title: 'Updated Twins Succeeded'
                      titleKind: 2
                      visualization: {
                        chartType: 5
                        legendVisualization: {
                          isVisible: true
                          position: 2
                          hideSubtitle: false
                        }
                        axisVisualization: {
                          x: {
                            isVisible: true
                            axisType: 2
                          }
                          y: {
                            isVisible: true
                            axisType: 1
                          }
                        }
                        disablePinning: true
                      }
                      filterCollection: {
                        filters: [
                          {
                            key: 'Action'
                            operator: 0
                            values: [
                              'Update'
                            ]
                          }
                          {
                            key: 'status'
                            operator: 0
                            values: [
                              'Succeeded'
                            ]
                          }
                        ]
                      }
                      grouping: {
                        dimension: 'ModelId'
                        sort: 2
                        top: 10
                      }
                    }
                  }
                }
              }
            }
          }
          {
            position: {
              x: 6
              y: 10
              rowSpan: 4
              colSpan: 6
            }
            metadata: {
              inputs: [
                {
                  name: 'options'
                  isOptional: true
                }
                {
                  name: 'sharedTimeRange'
                  isOptional: true
                }
              ]
              #disable-next-line BCP036
              type: 'Extension/HubsExtension/PartType/MonitorChartPart'
              settings: {
                content: {
                  options: {
                    chart: {
                      metrics: [
                        {
                          resourceMetadata: {
                            id: appInsights.id
                          }
                          name: 'Twins'
                          aggregationType: 1
                          namespace: 'azure.applicationinsights'
                          metricVisualization: {
                            displayName: 'Twins'
                            resourceDisplayName: appInsightsName
                          }
                        }
                      ]
                      title: 'Update Twins Failed'
                      titleKind: 2
                      visualization: {
                        chartType: 5
                        legendVisualization: {
                          isVisible: true
                          position: 2
                          hideSubtitle: false
                        }
                        axisVisualization: {
                          x: {
                            isVisible: true
                            axisType: 2
                          }
                          y: {
                            isVisible: true
                            axisType: 1
                          }
                        }
                        disablePinning: true
                      }
                      filterCollection: {
                        filters: [
                          {
                            key: 'Action'
                            operator: 0
                            values: [
                              'Update'
                            ]
                          }
                          {
                            key: 'status'
                            operator: 0
                            values: [
                              'Failed'
                            ]
                          }
                        ]
                      }
                      grouping: {
                        dimension: 'ModelId'
                        sort: 2
                        top: 10
                      }
                    }
                  }
                }
              }
            }
          }
          {
            position: {
              x: 12
              y: 10
              rowSpan: 4
              colSpan: 6
            }
            metadata: {
              inputs: [
                {
                  name: 'options'
                  isOptional: true
                }
                {
                  name: 'sharedTimeRange'
                  isOptional: true
                }
              ]
              #disable-next-line BCP036
              type: 'Extension/HubsExtension/PartType/MonitorChartPart'
              settings: {
                content: {
                  options: {
                    chart: {
                      metrics: [
                        {
                          resourceMetadata: {
                            id: appInsights.id
                          }
                          name: 'Twins'
                          aggregationType: 1
                          namespace: 'azure.applicationinsights'
                          metricVisualization: {
                            displayName: 'Twins'
                            resourceDisplayName: appInsightsName
                          }
                        }
                      ]
                      title: 'Update Twin - Throttled'
                      titleKind: 2
                      visualization: {
                        chartType: 5
                        legendVisualization: {
                          isVisible: true
                          position: 2
                          hideSubtitle: false
                        }
                        axisVisualization: {
                          x: {
                            isVisible: true
                            axisType: 2
                          }
                          y: {
                            isVisible: true
                            axisType: 1
                          }
                        }
                        disablePinning: true
                      }
                      filterCollection: {
                        filters: [
                          {
                            key: 'Action'
                            operator: 0
                            values: [
                              'Update'
                            ]
                          }
                          {
                            key: 'status'
                            operator: 0
                            values: [
                              'Throttled'
                            ]
                          }
                        ]
                      }
                      grouping: {
                        dimension: 'ModelId'
                        sort: 2
                        top: 10
                      }
                    }
                  }
                }
              }
            }
          }
          {
            position: {
              x: 18
              y: 10
              rowSpan: 4
              colSpan: 6
            }
            metadata: {
              inputs: [
                {
                  name: 'options'
                  isOptional: true
                }
                {
                  name: 'sharedTimeRange'
                  isOptional: true
                }
              ]
              #disable-next-line BCP036
              type: 'Extension/HubsExtension/PartType/MonitorChartPart'
              settings: {
                content: {
                  options: {
                    chart: {
                      metrics: [
                        {
                          resourceMetadata: {
                            id: appInsights.id
                          }
                          name: 'Twins'
                          aggregationType: 1
                          namespace: 'azure.applicationinsights'
                          metricVisualization: {
                            displayName: 'Twins'
                            resourceDisplayName: appInsightsName
                          }
                        }
                      ]
                      title: 'Update Twin - Unchanged'
                      titleKind: 2
                      visualization: {
                        chartType: 5
                        legendVisualization: {
                          isVisible: true
                          position: 2
                          hideSubtitle: false
                        }
                        axisVisualization: {
                          x: {
                            isVisible: true
                            axisType: 2
                          }
                          y: {
                            isVisible: true
                            axisType: 1
                          }
                        }
                        disablePinning: true
                      }
                      filterCollection: {
                        filters: [
                          {
                            key: 'Action'
                            operator: 0
                            values: [
                              'Update'
                            ]
                          }
                          {
                            key: 'status'
                            operator: 0
                            values: [
                              'Skipped'
                            ]
                          }
                        ]
                      }
                      grouping: {
                        dimension: 'ModelId'
                        sort: 2
                        top: 10
                      }

                    }
                  }
                }
              }
            }
          }
          {
            position: {
              x: 0
              y: 14
              rowSpan: 1
              colSpan: 24
            }
            metadata: {
              inputs: []
              type: 'Extension/HubsExtension/PartType/MarkdownPart'
              settings: {
                content: {
                  #disable-next-line BCP037
                  content: '## Relationships'
                  #disable-next-line BCP037
                  markdownSource: 1
                }
              }
            }
          }
          {
            position: {
              x: 0
              y: 15
              rowSpan: 4
              colSpan: 6
            }
            metadata: {
              inputs: [
                {
                  name: 'options'
                  isOptional: true
                }
                {
                  name: 'sharedTimeRange'
                  isOptional: true
                }
              ]
              #disable-next-line BCP036
              type: 'Extension/HubsExtension/PartType/MonitorChartPart'
              settings: {
                content: {
                  options: {
                    chart: {
                      metrics: [
                        {
                          resourceMetadata: {
                            id: appInsights.id
                          }
                          name: 'Relationships'
                          aggregationType: 1
                          namespace: 'azure.applicationinsights'
                          metricVisualization: {
                            displayName: 'Relationships'
                            resourceDisplayName: appInsightsName
                          }
                        }
                      ]
                      title: 'Created/Reset Relationships Succeeded'
                      titleKind: 2
                      visualization: {
                        chartType: 5
                        legendVisualization: {
                          isVisible: true
                          position: 2
                          hideSubtitle: false
                        }
                        axisVisualization: {
                          x: {
                            isVisible: true
                            axisType: 2
                          }
                          y: {
                            isVisible: true
                            axisType: 1
                          }
                        }
                        disablePinning: true
                      }
                      filterCollection: {
                        filters: [
                          {
                            key: 'Action'
                            operator: 0
                            values: [
                              'create'
                            ]
                          }
                          {
                            key: 'status'
                            operator: 0
                            values: [
                              'Succeeded'
                            ]
                          }
                        ]
                      }
                      grouping: {
                        dimension: 'RelationshipType'
                        sort: 2
                        top: 10
                      }
                    }
                  }
                }
              }
            }
          }
          {
            position: {
              x: 6
              y: 15
              rowSpan: 4
              colSpan: 6
            }
            metadata: {
              inputs: [
                {
                  name: 'options'
                  isOptional: true
                }
                {
                  name: 'sharedTimeRange'
                  isOptional: true
                }
              ]
              #disable-next-line BCP036
              type: 'Extension/HubsExtension/PartType/MonitorChartPart'
              settings: {
                content: {
                  options: {
                    chart: {
                      metrics: [
                        {
                          resourceMetadata: {
                            id: appInsights.id
                          }
                          name: 'Relationships'
                          aggregationType: 1
                          namespace: 'azure.applicationinsights'
                          metricVisualization: {
                            displayName: 'Relationships'
                            resourceDisplayName: appInsightsName
                          }
                        }
                      ]
                      title: 'Create/Reset Relationships Failed'
                      titleKind: 2
                      visualization: {
                        chartType: 5
                        legendVisualization: {
                          isVisible: true
                          position: 2
                          hideSubtitle: false
                        }
                        axisVisualization: {
                          x: {
                            isVisible: true
                            axisType: 2
                          }
                          y: {
                            isVisible: true
                            axisType: 1
                          }
                        }
                        disablePinning: true
                      }
                      filterCollection: {
                        filters: [
                          {
                            key: 'Action'
                            operator: 0
                            values: [
                              'create'
                            ]
                          }
                          {
                            key: 'status'
                            operator: 0
                            values: [
                              'Failed'
                            ]
                          }
                        ]
                      }
                      grouping: {
                        dimension: 'RelationshipType'
                        sort: 2
                        top: 10
                      }
                    }
                  }
                }
              }
            }
          }
          {
            position: {
              x: 12
              y: 15
              rowSpan: 4
              colSpan: 6
            }
            metadata: {
              inputs: [
                {
                  name: 'options'
                  isOptional: true
                }
                {
                  name: 'sharedTimeRange'
                  isOptional: true
                }
              ]
              #disable-next-line BCP036
              type: 'Extension/HubsExtension/PartType/MonitorChartPart'
              settings: {
                content: {
                  options: {
                    chart: {
                      metrics: [
                        {
                          resourceMetadata: {
                            id: appInsights.id
                          }
                          name: 'Relationships'
                          aggregationType: 1
                          namespace: 'azure.applicationinsights'
                          metricVisualization: {
                            displayName: 'Relationships'
                            resourceDisplayName: appInsightsName
                          }
                        }
                      ]
                      title: 'Create/Reset Relationship Throttled'
                      titleKind: 2
                      visualization: {
                        chartType: 5
                        legendVisualization: {
                          isVisible: true
                          position: 2
                          hideSubtitle: false
                        }
                        axisVisualization: {
                          x: {
                            isVisible: true
                            axisType: 2
                          }
                          y: {
                            isVisible: true
                            axisType: 1
                          }
                        }
                        disablePinning: true
                      }
                      filterCollection: {
                        filters: [
                          {
                            key: 'Action'
                            operator: 0
                            values: [
                              'create'
                            ]
                          }
                          {
                            key: 'status'
                            operator: 0
                            values: [
                              'Throttled'
                            ]
                          }
                        ]
                      }
                      grouping: {
                        dimension: 'RelationshipType'
                        sort: 2
                        top: 10
                      }
                    }
                  }
                }
              }
            }
          }
          {
            position: {
              x: 0
              y: 19
              rowSpan: 4
              colSpan: 6
            }
            metadata: {
              inputs: [
                {
                  name: 'options'
                  isOptional: true
                }
                {
                  name: 'sharedTimeRange'
                  isOptional: true
                }
              ]
              #disable-next-line BCP036
              type: 'Extension/HubsExtension/PartType/MonitorChartPart'
              settings: {
                content: {
                  options: {
                    chart: {
                      metrics: [
                        {
                          resourceMetadata: {
                            id: appInsights.id
                          }
                          name: 'Relationships'
                          aggregationType: 1
                          namespace: 'azure.applicationinsights'
                          metricVisualization: {
                            displayName: 'Relationships'
                            resourceDisplayName: appInsightsName
                          }
                        }
                      ]
                      title: 'Updated Relationships Succeeded'
                      titleKind: 2
                      visualization: {
                        chartType: 5
                        legendVisualization: {
                          isVisible: true
                          position: 2
                          hideSubtitle: false
                        }
                        axisVisualization: {
                          x: {
                            isVisible: true
                            axisType: 2
                          }
                          y: {
                            isVisible: true
                            axisType: 1
                          }
                        }
                        disablePinning: true
                      }
                      filterCollection: {
                        filters: [
                          {
                            key: 'Action'
                            operator: 0
                            values: [
                              'update'
                            ]
                          }
                          {
                            key: 'status'
                            operator: 0
                            values: [
                              'Succeeded'
                            ]
                          }
                        ]
                      }
                      grouping: {
                        dimension: 'RelationshipType'
                        sort: 2
                        top: 10
                      }
                    }
                  }
                }
              }
            }
          }
          {
            position: {
              x: 6
              y: 19
              rowSpan: 4
              colSpan: 6
            }
            metadata: {
              inputs: [
                {
                  name: 'options'
                  isOptional: true
                }
                {
                  name: 'sharedTimeRange'
                  isOptional: true
                }
              ]
              #disable-next-line BCP036
              type: 'Extension/HubsExtension/PartType/MonitorChartPart'
              settings: {
                content: {
                  options: {
                    chart: {
                      metrics: [
                        {
                          resourceMetadata: {
                            id: appInsights.id
                          }
                          name: 'Relationships'
                          aggregationType: 1
                          namespace: 'azure.applicationinsights'
                          metricVisualization: {
                            displayName: 'Relationships'
                            resourceDisplayName: appInsightsName
                          }
                        }
                      ]
                      title: 'Update Relationships Failed'
                      titleKind: 2
                      visualization: {
                        chartType: 5
                        legendVisualization: {
                          isVisible: true
                          position: 2
                          hideSubtitle: false
                        }
                        axisVisualization: {
                          x: {
                            isVisible: true
                            axisType: 2
                          }
                          y: {
                            isVisible: true
                            axisType: 1
                          }
                        }
                        disablePinning: true
                      }
                      filterCollection: {
                        filters: [
                          {
                            key: 'Action'
                            operator: 0
                            values: [
                              'update'
                            ]
                          }
                          {
                            key: 'status'
                            operator: 0
                            values: [
                              'Failed'
                            ]
                          }
                        ]
                      }
                      grouping: {
                        dimension: 'RelationshipType'
                        sort: 2
                        top: 10
                      }
                    }
                  }
                }
              }
            }
          }
          {
            position: {
              x: 12
              y: 19
              rowSpan: 4
              colSpan: 6
            }
            metadata: {
              inputs: [
                {
                  name: 'options'
                  isOptional: true
                }
                {
                  name: 'sharedTimeRange'
                  isOptional: true
                }
              ]
              #disable-next-line BCP036
              type: 'Extension/HubsExtension/PartType/MonitorChartPart'
              settings: {
                content: {
                  options: {
                    chart: {
                      metrics: [
                        {
                          resourceMetadata: {
                            id: appInsights.id
                          }
                          name: 'Relationships'
                          aggregationType: 1
                          namespace: 'azure.applicationinsights'
                          metricVisualization: {
                            displayName: 'Relationships'
                            resourceDisplayName: appInsightsName
                          }
                        }
                      ]
                      title: 'Update Relationships Throttled'
                      titleKind: 2
                      visualization: {
                        chartType: 5
                        legendVisualization: {
                          isVisible: true
                          position: 2
                          hideSubtitle: false
                        }
                        axisVisualization: {
                          x: {
                            isVisible: true
                            axisType: 2
                          }
                          y: {
                            isVisible: true
                            axisType: 1
                          }
                        }
                        disablePinning: true
                      }
                      filterCollection: {
                        filters: [
                          {
                            key: 'Action'
                            operator: 0
                            values: [
                              'Update'
                            ]
                          }
                          {
                            key: 'status'
                            operator: 0
                            values: [
                              'Throttled'
                            ]
                          }
                        ]
                      }
                      grouping: {
                        dimension: 'RelationshipType'
                        sort: 2
                        top: 10
                      }
                    }
                  }
                }
              }
            }
          }
          {
            position: {
              x: 18
              y: 19
              rowSpan: 4
              colSpan: 6
            }
            metadata: {
              inputs: [
                {
                  name: 'options'
                  isOptional: true
                }
                {
                  name: 'sharedTimeRange'
                  isOptional: true
                }
              ]
              #disable-next-line BCP036
              type: 'Extension/HubsExtension/PartType/MonitorChartPart'
              settings: {
                content: {
                  options: {
                    chart: {
                      metrics: [
                        {
                          resourceMetadata: {
                            id: appInsights.id
                          }
                          name: 'Relationships'
                          aggregationType: 1
                          namespace: 'azure.applicationinsights'
                          metricVisualization: {
                            displayName: 'Relationships'
                            resourceDisplayName: appInsightsName
                          }
                        }
                      ]
                      title: 'Update Relationships - Unchanged'
                      titleKind: 2
                      visualization: {
                        chartType: 5
                        legendVisualization: {
                          isVisible: true
                          position: 2
                          hideSubtitle: false
                        }
                        axisVisualization: {
                          x: {
                            isVisible: true
                            axisType: 2
                          }
                          y: {
                            isVisible: true
                            axisType: 1
                          }
                        }
                        disablePinning: true
                      }
                      filterCollection: {
                        filters: [
                          {
                            key: 'Action'
                            operator: 0
                            values: [
                              'update'
                            ]
                          }
                          {
                            key: 'status'
                            operator: 0
                            values: [
                              'Skipped'
                            ]
                          }
                        ]
                      }
                      grouping: {
                        dimension: 'RelationshipType'
                        sort: 2
                        top: 10
                      }
                    }
                  }
                }
              }
            }
          }
          {
            position: {
              x: 0
              y: 23
              rowSpan: 1
              colSpan: 24
            }
            metadata: {
              inputs: []
              type: 'Extension/HubsExtension/PartType/MarkdownPart'
              settings: {
                content: {
                  #disable-next-line BCP037
                  content: '# Ontology Mapping Metrics'
                  #disable-next-line BCP037
                  markdownSource: 1
                }
              }
            }
          }
          {
            position: {
              x: 0
              y: 24
              rowSpan: 4
              colSpan: 6
            }
            metadata: {
              inputs: [
                {
                  name: 'options'
                  isOptional: true
                }
                {
                  name: 'sharedTimeRange'
                  isOptional: true
                }
              ]
              #disable-next-line BCP036
              type: 'Extension/HubsExtension/PartType/MonitorChartPart'
              settings: {
                content: {
                  options: {
                    chart: {
                      metrics: [
                        {
                          resourceMetadata: {
                            id: appInsights.id
                          }
                          name: 'MappingForInputDtmiNotFound'
                          aggregationType: 1
                          namespace: 'azure.applicationinsights'
                          metricVisualization: {
                            displayName: 'MappingForInputDtmiNotFound'
                            resourceDisplayName: appInsightsName
                          }
                        }
                      ]
                      title: 'Mapping for Input DTMI Not Found'
                      titleKind: 2
                      visualization: {
                        chartType: 5
                        legendVisualization: {
                          isVisible: true
                          position: 2
                          hideSubtitle: false
                        }
                        axisVisualization: {
                          x: {
                            isVisible: true
                            axisType: 2
                          }
                          y: {
                            isVisible: true
                            axisType: 1
                          }
                        }
                        disablePinning: true
                      }
                      grouping: {
                        dimension: 'InterfaceType'
                        sort: 2
                        top: 10
                      }
                    }
                  }
                }
              }
            }
          }
          {
            position: {
              x: 6
              y: 24
              rowSpan: 4
              colSpan: 6
            }
            metadata: {
              inputs: [
                {
                  name: 'options'
                  isOptional: true
                }
                {
                  name: 'sharedTimeRange'
                  isOptional: true
                }
              ]
              #disable-next-line BCP036
              type: 'Extension/HubsExtension/PartType/MonitorChartPart'
              settings: {
                content: {
                  options: {
                    chart: {
                      metrics: [
                        {
                          resourceMetadata: {
                            id: appInsights.id
                          }
                          name: 'OutputMappingForInputDtmiNotFound'
                          aggregationType: 1
                          namespace: 'azure.applicationinsights'
                          metricVisualization: {
                            displayName: 'OutputMappingForInputDtmiNotFound'
                            resourceDisplayName: appInsightsName
                          }
                        }
                      ]
                      title: 'Output Mapping For Input Dtmi Not Found'
                      titleKind: 2
                      visualization: {
                        chartType: 5
                        legendVisualization: {
                          isVisible: true
                          position: 2
                          hideSubtitle: false
                        }
                        axisVisualization: {
                          x: {
                            isVisible: true
                            axisType: 2
                          }
                          y: {
                            isVisible: true
                            axisType: 1
                          }
                        }
                        disablePinning: true
                      }
                      grouping: {
                        dimension: 'OutputDtmi'
                        sort: 2
                        top: 10
                      }
                    }
                  }
                }
              }
            }
          }
          {
            position: {
              x: 12
              y: 24
              rowSpan: 4
              colSpan: 6
            }
            metadata: {
              inputs: [
                {
                  name: 'options'
                  isOptional: true
                }
                {
                  name: 'sharedTimeRange'
                  isOptional: true
                }
              ]
              #disable-next-line BCP036
              type: 'Extension/HubsExtension/PartType/MonitorChartPart'
              settings: {
                content: {
                  options: {
                    chart: {
                      metrics: [
                        {
                          resourceMetadata: {
                            id: appInsights.id
                          }
                          name: 'TargetDtmiNotFound'
                          aggregationType: 1
                          namespace: 'azure.applicationinsights'
                          metricVisualization: {
                            displayName: 'TargetDtmiNotFound'
                            resourceDisplayName: appInsightsName
                          }
                        }
                      ]
                      title: 'Target DTMI Not Found'
                      titleKind: 2
                      visualization: {
                        chartType: 5
                        legendVisualization: {
                          isVisible: true
                          position: 2
                          hideSubtitle: false
                        }
                        axisVisualization: {
                          x: {
                            isVisible: true
                            axisType: 2
                          }
                          y: {
                            isVisible: true
                            axisType: 1
                          }
                        }
                        disablePinning: true
                      }
                      grouping: {
                        dimension: 'InterfaceType'
                        sort: 2
                        top: 10
                      }
                    }
                  }
                }
              }
            }
          }
          {
            position: {
              x: 0
              y: 28
              rowSpan: 4
              colSpan: 6
            }
            metadata: {
              inputs: [
                {
                  name: 'options'
                  isOptional: true
                }
                {
                  name: 'sharedTimeRange'
                  isOptional: true
                }
              ]
              #disable-next-line BCP036
              type: 'Extension/HubsExtension/PartType/MonitorChartPart'
              settings: {
                content: {
                  options: {
                    chart: {
                      metrics: [
                        {
                          resourceMetadata: {
                            id: appInsights.id
                          }
                          name: 'InputInterfaceNotFound'
                          aggregationType: 1
                          namespace: 'azure.applicationinsights'
                          metricVisualization: {
                            displayName: 'InputInterfaceNotFound'
                            resourceDisplayName: appInsightsName
                          }
                        }
                      ]
                      title: 'InputInterfaceNotFound not found in Model by InterfaceType'
                      titleKind: 2
                      visualization: {
                        chartType: 5
                        legendVisualization: {
                          isVisible: true
                          position: 2
                          hideSubtitle: false
                        }
                        axisVisualization: {
                          x: {
                            isVisible: true
                            axisType: 2
                          }
                          y: {
                            isVisible: true
                            axisType: 1
                          }
                        }
                        disablePinning: true
                      }
                      grouping: {
                        dimension: 'InterfaceType'
                        sort: 2
                        top: 10
                      }
                    }
                  }
                }
              }
            }
          }
          {
            position: {
              x: 6
              y: 28
              rowSpan: 4
              colSpan: 6
            }
            metadata: {
              inputs: [
                {
                  name: 'options'
                  isOptional: true
                }
                {
                  name: 'sharedTimeRange'
                  isOptional: true
                }
              ]
              #disable-next-line BCP036
              type: 'Extension/HubsExtension/PartType/MonitorChartPart'
              settings: {
                content: {
                  options: {
                    chart: {
                      metrics: [
                        {
                          resourceMetadata: {
                            id: appInsights.id
                          }
                          name: 'RelationshipNotFoundInModel'
                          aggregationType: 1
                          namespace: 'azure.applicationinsights'
                          metricVisualization: {
                            displayName: 'RelationshipNotFoundInModel'
                            resourceDisplayName: appInsightsName
                          }
                        }
                      ]
                      title: 'Relationship Not Found in Model by RelationshipType'
                      titleKind: 2
                      visualization: {
                        chartType: 5
                        legendVisualization: {
                          isVisible: true
                          position: 2
                          hideSubtitle: false
                        }
                        axisVisualization: {
                          x: {
                            isVisible: true
                            axisType: 2
                          }
                          y: {
                            isVisible: true
                            axisType: 1
                          }
                        }
                        disablePinning: true
                      }
                      grouping: {
                        dimension: 'RelationshipType'
                        sort: 2
                        top: 10
                      }
                    }
                  }
                }
              }
            }
          }
          {
            position: {
              x: 12
              y: 28
              rowSpan: 4
              colSpan: 6
            }
            metadata: {
              inputs: [
                {
                  name: 'options'
                  isOptional: true
                }
                {
                  name: 'sharedTimeRange'
                  isOptional: true
                }
              ]
              #disable-next-line BCP036
              type: 'Extension/HubsExtension/PartType/MonitorChartPart'
              settings: {
                content: {
                  options: {
                    chart: {
                      metrics: [
                        {
                          resourceMetadata: {
                            id: appInsights.id
                          }
                          name: 'DuplicateMappingPropertyFound'
                          aggregationType: 1
                          namespace: 'azure.applicationinsights'
                          metricVisualization: {
                            displayName: 'DuplicateMappingPropertyFound'
                            resourceDisplayName: appInsightsName
                          }
                        }
                      ]
                      title: 'Duplicate property found in Model by PropertyName'
                      titleKind: 2
                      visualization: {
                        chartType: 5
                        legendVisualization: {
                          isVisible: true
                          position: 2
                          hideSubtitle: false
                        }
                        axisVisualization: {
                          x: {
                            isVisible: true
                            axisType: 2
                          }
                          y: {
                            isVisible: true
                            axisType: 1
                          }
                        }
                        disablePinning: true
                      }
                      grouping: {
                        dimension: 'PropertyName'
                        sort: 2
                        top: 10
                      }
                    }
                  }
                }
              }
            }
          }
          {
            position: {
              x: 0
              y: 32
              rowSpan: 4
              colSpan: 6
            }
            metadata: {
              inputs: [
                {
                  name: 'options'
                  isOptional: true
                }
                {
                  name: 'sharedTimeRange'
                  isOptional: true
                }
              ]
              #disable-next-line BCP036
              type: 'Extension/HubsExtension/PartType/MonitorChartPart'
              settings: {
                content: {
                  options: {
                    chart: {
                      metrics: [
                        {
                          resourceMetadata: {
                            id: appInsights.id
                          }
                          name: 'InvalidTargetDtmis'
                          aggregationType: 1
                          namespace: 'azure.applicationinsights'
                          metricVisualization: {
                            displayName: 'InvalidTargetDtmis'
                            resourceDisplayName: appInsightsName
                          }
                        }
                      ]
                      title: 'Invalid Target DTMI Mappings in MappingFile'
                      titleKind: 2
                      visualization: {
                        chartType: 5
                        legendVisualization: {
                          isVisible: true
                          position: 2
                          hideSubtitle: false
                        }
                        axisVisualization: {
                          x: {
                            isVisible: true
                            axisType: 2
                          }
                          y: {
                            isVisible: true
                            axisType: 1
                          }
                        }
                        disablePinning: true
                      }
                    }
                  }
                }
              }
            }
          }
          {
            position: {
              x: 6
              y: 32
              rowSpan: 4
              colSpan: 6
            }
            metadata: {
              inputs: [
                {
                  name: 'options'
                  isOptional: true
                }
                {
                  name: 'sharedTimeRange'
                  isOptional: true
                }
              ]
              #disable-next-line BCP036
              type: 'Extension/HubsExtension/PartType/MonitorChartPart'
              settings: {
                content: {
                  options: {
                    chart: {
                      metrics: [
                        {
                          resourceMetadata: {
                            id: appInsights.id
                          }
                          name: 'InvalidOutputDtmi'
                          aggregationType: 1
                          namespace: 'azure.applicationinsights'
                          metricVisualization: {
                            displayName: 'InvalidOutputDtmi'
                            resourceDisplayName: appInsightsName
                          }
                        }
                      ]
                      title: 'Invalid Output DTMI by OutputDtmi'
                      titleKind: 2
                      visualization: {
                        chartType: 5
                        legendVisualization: {
                          isVisible: true
                          position: 2
                          hideSubtitle: false
                        }
                        axisVisualization: {
                          x: {
                            isVisible: true
                            axisType: 2
                          }
                          y: {
                            isVisible: true
                            axisType: 1
                          }
                        }
                        disablePinning: true
                      }
                      grouping: {
                        dimension: 'InvalidOutputDtmi'
                        sort: 2
                        top: 10
                      }
                    }
                  }
                }
              }
            }
          }
        ]
      }
    ]
    metadata: {
      model: {
        timeRange: {
          value: {
            relative: {
              duration: 24
              timeUnit: 1
            }
          }
          type: 'MsPortalFx.Composition.Configuration.ValueTypes.TimeRange'
        }
        filterLocale: {
          value: 'en-us'
        }
        filters: {
          value: {
            MsPortalFx_TimeRange: {
              model: {
                format: 'local'
                granularity: 'auto'
                relative: '12h'
              }
              displayCache: {
                name: 'Local Time'
                value: 'Past 12 hours'
              }
              filteredPartIds: [
                'StartboardPart-MonitorChartPart-dfe569cf-bfe3-489e-ad3e-b23c3822bd66'
                'StartboardPart-MonitorChartPart-dfe569cf-bfe3-489e-ad3e-b23c3822bd68'
                'StartboardPart-MonitorChartPart-dfe569cf-bfe3-489e-ad3e-b23c3822bd6a'
                'StartboardPart-MonitorChartPart-dfe569cf-bfe3-489e-ad3e-b23c3822bd6c'
                'StartboardPart-MonitorChartPart-dfe569cf-bfe3-489e-ad3e-b23c3822bd6e'
                'StartboardPart-MonitorChartPart-dfe569cf-bfe3-489e-ad3e-b23c3822bd70'
                'StartboardPart-MonitorChartPart-dfe569cf-bfe3-489e-ad3e-b23c3822bd72'
                'StartboardPart-MonitorChartPart-dfe569cf-bfe3-489e-ad3e-b23c3822bd74'
                'StartboardPart-MonitorChartPart-dfe569cf-bfe3-489e-ad3e-b23c3822bd76'
                'StartboardPart-MonitorChartPart-dfe569cf-bfe3-489e-ad3e-b23c3822bd78'
                'StartboardPart-MonitorChartPart-dfe569cf-bfe3-489e-ad3e-b23c3822bd7c'
                'StartboardPart-MonitorChartPart-dfe569cf-bfe3-489e-ad3e-b23c3822bd7e'
                'StartboardPart-MonitorChartPart-dfe569cf-bfe3-489e-ad3e-b23c3822bd80'
                'StartboardPart-MonitorChartPart-dfe569cf-bfe3-489e-ad3e-b23c3822bd82'
                'StartboardPart-MonitorChartPart-dfe569cf-bfe3-489e-ad3e-b23c3822be51'
                'StartboardPart-MonitorChartPart-dfe569cf-bfe3-489e-ad3e-b23c3822bfca'
              ]
            }
          }
        }
      }
    }
  }
}

output dashboardName string = dashboardName

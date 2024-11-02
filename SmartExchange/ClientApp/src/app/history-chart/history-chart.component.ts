import {
  AfterViewInit,
  Component,
  Input,
  OnDestroy,
  OnChanges,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import * as echarts from 'echarts';

@Component({
  selector: 'app-history-chart',
  templateUrl: './history-chart.component.html',
  styleUrls: ['./history-chart.component.css'],
  standalone: true,
  imports: [CommonModule, FormsModule],
})
export class HistoryChartComponent
  implements AfterViewInit, OnDestroy, OnChanges
{
  @Input() historyAssets: any[] = [];
  activeTab: string = '';
  excludeSmallAmounts: boolean = false;

  private chartInstances: any[] = [];

  ngAfterViewInit(): void {
    if (this.historyAssets.length > 0) {
      this.activeTab = this.historyAssets[0].name;
      setTimeout(() => {
        this.renderActiveChart();
      });
    }
  }

  ngOnChanges(): void {
    this.renderActiveChart();
  }

  renderActiveChart(): void {
    const activeAsset = this.historyAssets.find(
      (asset) => asset.name === this.activeTab
    );
    if (activeAsset) {
      this.disposeActiveCharts();

      setTimeout(() => {
        const chartContainer = document.getElementById(
          `chart-container-${this.activeTab}`
        );
        if (chartContainer) {
          const chartInstance = echarts.init(chartContainer);
          this.createAssetChart(chartInstance, activeAsset);
          this.chartInstances.push(chartInstance);
        }
      });
    }
  }

  private createAssetChart(chartInstance: any, asset: any): void {
    let chartData = asset.items.map(
      (item: { dateCreated: string | number | Date; quantity: any }) => {
        const date = new Date(item.dateCreated)
          .toISOString()
          .slice(0, 19)
          .replace('T', ' ');
        const quantity = parseFloat(item.quantity);
        return { date, quantity };
      }
    );

    if (this.excludeSmallAmounts) {
      const totalQuantity = chartData.reduce(
        (sum: number, dataPoint: { quantity: number }) =>
          sum + dataPoint.quantity,
        0
      );
      const averageQuantity = totalQuantity / chartData.length;
      chartData = chartData.filter(
        (dataPoint: { quantity: number }) =>
          dataPoint.quantity > averageQuantity
      );
    }

    const dates = chartData.map((dataPoint: { date: any }) => dataPoint.date);
    const quantities = chartData.map(
      (dataPoint: { quantity: any }) => dataPoint.quantity
    );

    const option = {
      title: {
        text: `${asset.name} Quantity Over Time`,
      },
      tooltip: {
        trigger: 'axis',
        axisPointer: {
          type: 'cross',
        },
      },
      xAxis: {
        type: 'category',
        data: dates,
        name: 'Date & Time',
        axisLabel: {
          formatter: (value: string) => value,
        },
      },
      yAxis: {
        type: 'value',
        name: 'Quantity',
        min: 0,
        max: this.excludeSmallAmounts
          ? Math.max(...quantities) * 1.1
          : Math.max(...quantities) * 1.2,
        axisLabel: {
          formatter: (value: number) => value.toFixed(2),
        },
      },
      series: [
        {
          name: `${asset.name} Quantity`,
          type: 'line',
          data: quantities,
          itemStyle: {
            color: '#000000',
          },
          smooth: true,
          animation: false,
        },
      ],
      animation: false,
    };

    chartInstance.setOption(option);
  }

  private disposeActiveCharts(): void {
    this.chartInstances.forEach((instance) => {
      if (instance) {
        instance.dispose();
      }
    });
    this.chartInstances = [];
  }

  changeTab(assetName: string): void {
    this.activeTab = assetName;
    this.renderActiveChart();
  }

  ngOnDestroy(): void {
    this.disposeActiveCharts();
  }

  onCheckboxChange(): void {
    this.renderActiveChart();
  }
}

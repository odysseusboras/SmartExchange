import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ConnectionStatus, SignalRService } from './signal-r.service';
import { HistoryChartComponent } from './history-chart/history-chart.component';

@Component({
  selector: 'app-root',
  standalone: true,
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
  imports: [CommonModule, HistoryChartComponent],
})
export class AppComponent {
  data: any;
  historyData: any = {};
  connectionStatus: ConnectionStatus = ConnectionStatus.Disconnected;
  activeTab: string = 'tradingPairs';

  constructor(public signalRService: SignalRService) {
    this.signalRService.messages$.subscribe((message: any) => {
      this.data = message;
      console.log(this.data);
    });

    this.signalRService.connectionStatus$.subscribe(
      (status: ConnectionStatus) => {
        this.connectionStatus = status;
      }
    );
  }

  prepareHistoryData() {
    if (this.data && this.data.assetsHistory) {
      this.data.assetsHistory.forEach((asset: any) => {
        const labels = asset.items.map(
          (item: any) => new Date(item.dateCreatedLocal)
        );
        const quantities = asset.items.map((item: any) => item.quantity);

        this.historyData[asset.name] = {
          labels,
          quantities,
        };
      });
    }
  }
}

import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TaskItemStatus, TaskPriority, TaskStatusLabel, TaskPriorityLabel } from '../../core/models/task.models';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [CommonModule],
  template: `
    <span [class]="badgeClass">{{ label }}</span>
  `
})
export class StatusBadgeComponent {
  @Input() status!: TaskItemStatus;

  get label(): string {
    return TaskStatusLabel[this.status] ?? '';
  }

  get badgeClass(): string {
    const map: Record<TaskItemStatus, string> = {
      [TaskItemStatus.Pending]:    'badge badge-gray',
      [TaskItemStatus.InProgress]: 'badge badge-blue',
      [TaskItemStatus.InReview]:   'badge badge-purple',
      [TaskItemStatus.Completed]:  'badge badge-green',
      [TaskItemStatus.Cancelled]:  'badge badge-red'
    };
    return map[this.status] ?? 'badge badge-gray';
  }
}

@Component({
  selector: 'app-priority-badge',
  standalone: true,
  imports: [CommonModule],
  template: `
    <span [class]="badgeClass">{{ label }}</span>
  `
})
export class PriorityBadgeComponent {
  @Input() priority!: TaskPriority;

  get label(): string {
    return TaskPriorityLabel[this.priority] ?? '';
  }

  get badgeClass(): string {
    const map: Record<TaskPriority, string> = {
      [TaskPriority.Low]:    'badge badge-gray',
      [TaskPriority.Medium]: 'badge badge-blue',
      [TaskPriority.High]:   'badge badge-yellow',
      [TaskPriority.Urgent]: 'badge badge-red'
    };
    return map[this.priority] ?? 'badge badge-gray';
  }
}

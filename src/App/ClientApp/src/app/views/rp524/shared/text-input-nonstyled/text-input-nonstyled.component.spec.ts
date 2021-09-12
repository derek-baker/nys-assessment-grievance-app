import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TextInputNonstyledComponent } from './text-input-nonstyled.component';

describe('TextInputNonstyledComponent', () => {
  let component: TextInputNonstyledComponent;
  let fixture: ComponentFixture<TextInputNonstyledComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TextInputNonstyledComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TextInputNonstyledComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

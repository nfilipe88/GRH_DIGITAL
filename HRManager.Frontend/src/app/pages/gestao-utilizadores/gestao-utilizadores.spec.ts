import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GestaoUtilizadores } from './gestao-utilizadores';

describe('GestaoUtilizadores', () => {
  let component: GestaoUtilizadores;
  let fixture: ComponentFixture<GestaoUtilizadores>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GestaoUtilizadores]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GestaoUtilizadores);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
